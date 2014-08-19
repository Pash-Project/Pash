using System;
using System.Linq;
using System.Collections.Generic;
using System.Management;
using System.IO;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal delegate IEnumerable<string> ExpansionProviderFunction(string cmdStart, string replacableEnd);

    internal class TabExpansionProvider
    {
        private CommandManager _commandManager;
        private char[] _quoteChars = new char[] { '`', '\'', '"'};
        private ExpansionProviderFunction[] _expansionProviders;

        public TabExpansionProvider(CommandManager cmdManager)
        {
            _commandManager = cmdManager;
            _expansionProviders = new ExpansionProviderFunction[] {
                GetCommandExpansions,
                GetFilesystemExpansions
            };
        }

        public string[] DoTabExpansion(string cmdStart, string replacableEnd)
        {
            var expansions = new List<string>();

            foreach (var expProvider in _expansionProviders)
            {
                expansions.AddRange(expProvider(cmdStart, replacableEnd));
            }

            var expArray = expansions.ToArray();
            Array.Sort(expArray);
            return expArray;
        }

        private IEnumerable<string> GetCommandExpansions(string cmdStart, string replacableEnd)
        {
            if (_commandManager != null && String.IsNullOrEmpty(cmdStart))
            {
                return from cmd in _commandManager.FindCommands(replacableEnd + "*") select cmd.Name + " ";
            }
            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetFilesystemExpansions(string cmdStart, string replacableEnd)
        {
            replacableEnd = StripQuotes(replacableEnd);
            var dirCmd = String.IsNullOrEmpty(cmdStart) ? "cd " : cmdStart;

            var p = new System.Management.Path(replacableEnd).NormalizeSlashes();
            var startPath = new System.Management.Path(".");
            string lookFor = replacableEnd;
            if (replacableEnd.Contains(p.CorrectSlash))
            {
                // we already deal with a path
                if (!p.HasDrive() && !p.StartsWith("."))
                {
                    p = new System.Management.Path(".").Combine(p);
                }
                if (p.EndsWithSlash())
                {
                    startPath = p;
                    lookFor = "";
                }
                else
                {
                    startPath = p.GetParentPath(null);
                    lookFor = p.GetChildNameOrSelfIfNoChild();
                }
            }
            var dirinfo = new DirectoryInfo(startPath);
            if (!dirinfo.Exists)
            {
                return Enumerable.Empty<string>();
            }
            var expansions = new List<string>();
            bool includeHidden = lookFor.StartsWith(".");
            var pattern = new WildcardPattern(lookFor + "*");
            // add directories
            expansions.AddRange(
                from subdir in dirinfo.GetDirectories()
                where pattern.IsMatch(subdir.Name)
                select dirCmd + QuoteIfNecessary(startPath.Combine(subdir.Name).AppendSlashAtEnd())
            );
            // add files
            expansions.AddRange(
                from subdir in dirinfo.GetFiles()
                where pattern.IsMatch(subdir.Name)
                select QuoteIfNecessary(startPath.Combine(subdir.Name))
            );
            return expansions;
        }

        private string QuoteIfNecessary(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return "";
            }
            bool isQuoted = _quoteChars.Contains(str[0]);
            return str.Contains(' ') && !isQuoted ? String.Format("'{0}'", str) : str;
        }

        private string StripQuotes(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return "";
            }
            if (_quoteChars.Contains(str[0]))
            {
                str = str.Substring(1);
            }
            if (str.Length == 0)
            {
                return "";
            }
            if (_quoteChars.Contains(str[str.Length - 1]))
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
}


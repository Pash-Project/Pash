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
            char firstChar = replacableEnd.Length < 1 ? (char) 0 : replacableEnd[0];

            replacableEnd = StripQuotes(replacableEnd);

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
            var pattern = new WildcardPattern(lookFor + "*");
            // add directories
            expansions.AddRange(
                from subdir in dirinfo.GetDirectories()
                where pattern.IsMatch(subdir.Name)
                select startPath.Combine(subdir.Name).AppendSlashAtEnd().ToString()
            );
            // add files
            expansions.AddRange(
                from subdir in dirinfo.GetFiles()
                where pattern.IsMatch(subdir.Name)
                select startPath.Combine(subdir.Name).ToString()
            );
            for (int i = 0; i < expansions.Count; i++)
            {
                if (expansions[i].Contains(' '))
                {
                    expansions[i] = String.Format("'{0}'", expansions[i]);
                }
            }
            return expansions;
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


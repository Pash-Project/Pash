using System;
using System.Linq;
using System.Collections.Generic;
using System.Management;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Pash.Implementation
{
    internal class TabExpansionProvider
    {
        private LocalRunspace _runspace;
        private char[] _quoteChars = new char[] {'\'', '"'};

        public TabExpansionProvider(LocalRunspace runspace)
        {
            _runspace = runspace;
        }

        public string[] DoTabExpansion(string cmdStart, string replacableEnd)
        {
            var expansions = new List<string>();

            expansions.AddRange(GetCmdletParameterExpansion(cmdStart, replacableEnd));
            expansions.AddRange(GetCommandExpansions(cmdStart, replacableEnd));
            expansions.AddRange(GetFilesystemExpansions(cmdStart, replacableEnd));
            if (_runspace != null)
            {
                expansions.AddRange(GetFunctionExpansions(cmdStart, replacableEnd));
                expansions.AddRange(GetVariableExpansions(cmdStart, replacableEnd));
            }
            var expArray = expansions.ToArray();
            return expArray;
        }

        private IEnumerable<string> GetCmdletParameterExpansion(string cmdStart, string replacableEnd)
        {
            // either we try to find all parameters or a specific one
            if (!replacableEnd.StartsWith("-") && replacableEnd.Length > 0)
            {
                return Enumerable.Empty<string>();
            }
            // first find out by checking cmdStart if we're in a cmdlet by scanning from right to left for a cmdlet name
            var cmdRest = cmdStart.Trim();
            while (cmdRest.Length > 0)
            {
                int pos = cmdRest.LastUnquotedIndexOf(' ') + 1;
                var rest = cmdRest.Substring(pos).Trim();
                if (rest.StartsWith("|") || rest.StartsWith("(") || rest.StartsWith(";"))
                {
                    break;
                }
                if (Regex.IsMatch(rest, @"^\w+[\w-]+$"))
                {
                    var cmds = _runspace.CommandManager.FindCommands(rest);
                    var numCmds = cmds.Count();
                    if (numCmds > 1)
                    {
                        break;
                    }
                    else if (numCmds == 1)
                    {
                        var cmd = cmds.First() as CmdletInfo;
                        if (cmd != null) // found a cmdlet
                        {
                            // get all matching parameters
                            return GetCmdletParameterExpansions(cmd, replacableEnd);
                        }
                    }
                }
                cmdRest = cmdRest.Substring(0, pos -1);
            }
            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetCmdletParameterExpansions(CmdletInfo info, string prefix)
        {

            if (prefix.StartsWith("-"))
            {
                // get only a specific
                prefix = prefix.Substring(1);
            }
            else if (prefix.Length > 0)
            {
                // we only deal with empty prefix or those that began with "-"
                return Enumerable.Empty<string>();
            }
            var pattern = new WildcardPattern(prefix + "*", WildcardOptions.IgnoreCase);
            return from key in info.ParameterInfoLookupTable.Keys where pattern.IsMatch(key)
                orderby key ascending select "-" + key;
        }

        private IEnumerable<string> GetCommandExpansions(string cmdStart, string replacableEnd)
        {
            return from cmd in _runspace.CommandManager.FindCommands(replacableEnd + "*")
                orderby cmd.Name ascending select cmd.Name + " ";
        }

        private IEnumerable<string> GetVariableExpansions(string cmdStart, string replacableEnd)
        {
            // TODO: add support for scope prefixes like "global:"
            if (!replacableEnd.StartsWith("$"))
            {
                return Enumerable.Empty<string>();
            }
            replacableEnd = replacableEnd.Substring(1);
            var pattern = new WildcardPattern(replacableEnd + "*", WildcardOptions.IgnoreCase);
            var varnames = _runspace.ExecutionContext.SessionState.PSVariable.GetAll().Keys;
            return from vname in varnames where pattern.IsMatch(vname)
                orderby vname ascending select "$" + vname;
        }

        private IEnumerable<string> GetFunctionExpansions(string cmdStart, string replacableEnd)
        {
            // TODO: add support for scope prefixes like "global:"
            var pattern = new WildcardPattern(replacableEnd + "*", WildcardOptions.IgnoreCase);
            var funnames = _runspace.ExecutionContext.SessionState.Function.GetAll().Keys;
            return from fun in funnames where pattern.IsMatch(fun) orderby fun ascending select fun;
        }

        private IEnumerable<string> GetFilesystemExpansions(string cmdStart, string replacableEnd)
        {
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
            bool allowHidden = lookFor.Length > 0;

            // add directories
            expansions.AddRange(
                from subdir in dirinfo.GetDirectories()
                where pattern.IsMatch(subdir.Name) &&
                      (allowHidden || (subdir.Attributes & FileAttributes.Hidden) == 0)
                orderby subdir.Name ascending
                select QuoteIfNecessary(startPath.Combine(subdir.Name).AppendSlashAtEnd())
            );
            // add files
            expansions.AddRange(
                from file in dirinfo.GetFiles()
                where pattern.IsMatch(file.Name) &&
                (allowHidden || (file.Attributes & FileAttributes.Hidden) == 0)
                orderby file.Name ascending
                select QuoteIfNecessary(startPath.Combine(file.Name))
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


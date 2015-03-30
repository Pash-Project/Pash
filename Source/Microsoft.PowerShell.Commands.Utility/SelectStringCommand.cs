// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute(VerbsCommon.Select, "String", DefaultParameterSetName = "File" /*HelpUri="http://go.microsoft.com/fwlink/?LinkID=113388"*/)] 
    [OutputType(typeof(MatchInfo), typeof(bool))]
    public sealed class SelectStringCommand : PSCmdlet
    {
        [Parameter]
        public SwitchParameter AllMatches { get; set; }

        [Parameter]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter]
        [ValidateCount(1, 2)]
        [ValidateNotNullOrEmpty]
        [ValidateRange(0, 2147483647)]
        public int[] Context { get; set; }

        [Parameter]
        [ValidateSet(
            "unicode",
            "utf7",
            "utf8",
            "utf32",
            "ascii",
            "bigendianunicode",
            "default",
            "oem")]
        [ValidateNotNullOrEmpty]
        public string Encoding { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string[] Exclude { get; set; }

        [ParameterAttribute]
        [ValidateNotNullOrEmpty]
        public string[] Include { get; set; }

        [Parameter(
            ValueFromPipeline = true,
            Mandatory = true,
            ParameterSetName = "Object")]
        [AllowNull]
        [AllowEmptyString]
        public PSObject InputObject { get; set; }

        [Parameter]
        public SwitchParameter List { get; set; }

        [ParameterAttribute(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "LiteralFile")]
        [Alias("PSPath")]
        public string[] LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter NotMatch { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "File")]
        public string[] Path { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 0)]
        public string[] Pattern { get; set; }

        [Parameter]
        public SwitchParameter Quiet { get; set; }

        [Parameter]
        public SwitchParameter SimpleMatch { get; set; }

        int _lineNumber = 1;
        List<MatchInfo> _savedMatches = new List<MatchInfo>();

        protected override void ProcessRecord()
        {
            if (Path != null)
            {
                MatchInFiles();
            }
            else if (InputObject != null)
            {
                MatchInputObject();
            }
        }

        private void MatchInFiles()
        {
            foreach (string path in Path)
            {
                MatchInLines(path, File.ReadLines(path));
            }
        }

        /// <summary>
        /// When passing an array of strings using InputObject the array is turned into a single string
        /// with a space between each item of the array.
        /// 
        /// When an array of strings is passed down the pipeline each item of the array is searched
        /// individually.
        /// 
        /// https://technet.microsoft.com/en-us/library/hh849903.aspx
        /// </summary>
        private void MatchInputObject()
        {
            MatchInLines("InputStream", GetLines(InputObject));
        }

        protected override void EndProcessing()
        {
            foreach (MatchInfo match in _savedMatches)
            {
                WriteObject(match);
            }
        }

        private IEnumerable<string> GetLines(PSObject psObject)
        {
            var array = psObject.BaseObject as Array;
            if (array != null)
            {
                yield return String.Join(" ", array.OfType<object>().Select(item => item.ToString()));
            }
            yield return psObject.BaseObject.ToString();
        }

        private void MatchInLines(string path, IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                foreach (string pattern in Pattern)
                {
                    MatchInfo matchInfo = FindMatch(line, pattern, path);
                    if (matchInfo != null)
                    {
                        WriteObject(matchInfo);
                        break;
                    }
                }
                _lineNumber++;
            }
        }

        MatchInfo FindMatch(string line, string pattern, string path)
        {
            if (SimpleMatch)
            {
                return FindSimpleMatch(line, pattern, path);
            }
            return FindRegexMatch(line, pattern, path);
        }

        private MatchInfo FindRegexMatch(string line, string pattern, string path)
        {
            Match match = Regex.Match(line, pattern, GetRegexOptions());
            if (match.Success)
            {
                return new MatchInfo(path, pattern, match, line, _lineNumber, !CaseSensitive);
            }
            return null;
        }

        private MatchInfo FindSimpleMatch(string line, string pattern, string path)
        {
            if (line.IndexOf(pattern, GetStringComparison()) >= 0)
            {
                return new MatchInfo(path, pattern, line, _lineNumber, !CaseSensitive);
            }
            return null;
        }

        private RegexOptions GetRegexOptions()
        {
            if (CaseSensitive)
            {
                return RegexOptions.None;
            }
            return RegexOptions.IgnoreCase;
        }

        private StringComparison GetStringComparison()
        {
            if (CaseSensitive)
            {
                return StringComparison.CurrentCulture;
            }
            return StringComparison.CurrentCultureIgnoreCase;
        }
    }
}

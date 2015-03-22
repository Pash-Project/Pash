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
        /// TODO: How to determine whether InputObject is used or the pipeline is used?
        /// https://technet.microsoft.com/en-us/library/hh849903.aspx
        /// </summary>
        private void MatchInputObject()
        {
            MatchInLines("InputStream", GetLines(InputObject));
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
            int lineNumber = 1;
            foreach (string line in lines)
            {
                foreach (string pattern in Pattern)
                {
                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        var matchInfo = new MatchInfo(path, pattern, match, line, lineNumber);
                        WriteObject(matchInfo);
                        break;
                    }
                }
                lineNumber++;
            }
        }
    }
}

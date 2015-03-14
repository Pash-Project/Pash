// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "Command", DefaultParameterSetName = "CmdletSet")]
    public sealed class GetCommandCommand : PSCmdlet
    {
        [AllowEmptyCollection]
        [Parameter(Position = 1, ValueFromRemainingArguments = true)]
        [AllowNull]
        public object[] ArgumentList { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public CommandTypes CommandType { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
        public string[] Noun { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
        public string[] PSSnapin { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Syntax { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int TotalCount { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
        public string[] Verb { get; set; }

        protected override void ProcessRecord()
        {
            foreach (CmdletInfo cmdletInfo in GetCmdlets())
            {
                WriteObject(cmdletInfo);
            }
        }

        private IEnumerable<CmdletInfo> GetCmdlets()
        {
            if (Name != null)
            {
                return GetCmdlets(Name);
            }
            return GetCmdlets(new string[] {"*"});
        }

        private IEnumerable<CmdletInfo> GetCmdlets(string[] names)
        {
            foreach (string name in names)
            {
                foreach (KeyValuePair<string, CmdletInfo> cmdletInfo in ExecutionContext.SessionState.Cmdlet.Find(name))
                {
                    yield return cmdletInfo.Value;
                }
            }
        }
    }
}

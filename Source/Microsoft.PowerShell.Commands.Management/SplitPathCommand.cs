// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Currently borked, needs Pash.Engine implementation

using System;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Split", "Path")]
    public class SplitPathCommand : CoreCommandWithCredentialsBase
    {
        protected override void ProcessRecord()
        {
            foreach (Path path in Path)
            {
                WriteObject(path.GetParentPath(string.Empty).ToString());
            }
        }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true),
        Alias(new string[] { "PSPath" })]
        public string[] Path { get; set; }

        [Parameter]
        public SwitchParameter Resolve { get; set; }
    }
}
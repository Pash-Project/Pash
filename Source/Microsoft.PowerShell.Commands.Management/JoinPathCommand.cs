// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Currently borked, needs Pash.Engine implementation

using System;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Join", "Path")]
    public class JoinPathCommand : ProviderCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (Path parentPath in Path)
            {
                WriteObject(parentPath.Combine(ChildPath));
            }
        }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true),
        AllowNull,
        AllowEmptyString]
        public string ChildPath { get; set; }


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
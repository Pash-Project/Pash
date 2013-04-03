// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Copy", "Item", DefaultParameterSetName = "Path", SupportsShouldProcess = true)]
    public class CopyItemCommand : ProviderCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (String _path in Path)
            {
                InvokeProvider.Item.Copy(_path, Destination, Recurse.ToBool(), 
                    (Container.ToBool()) ? CopyContainers.CopyTargetContainer : CopyContainers.CopyChildrenOfTargetContainer);

                if (PassThru.ToBool()) WriteObject(Path);
            }
        }

        [Parameter]
        public SwitchParameter Container { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        [Alias(new string[] { "PSPath" }), 
        Parameter(
            ParameterSetName = "LiteralPath", 
            Position = 0, 
            Mandatory = true, 
            ValueFromPipeline = false, 
            ValueFromPipelineByPropertyName = true)]
        public string[] LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(
            ParameterSetName = "Path", 
            Position = 0, 
            Mandatory = true, 
            ValueFromPipeline = true, 
            ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        //protected override bool ProviderSupportsShouldProcess { get; }
    }
}

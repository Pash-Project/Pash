// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Rename", "Item", SupportsShouldProcess = true)]
    public class RenameItemCommand : CoreCommandWithCredentialsBase
    {
        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Rename(Path, NewName);

            if (PassThru.ToBool()) WriteObject(Path);
        }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string NewName { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Alias(new string[] { "PSPath" }),
        Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Path { get; set; }

        //protected override bool ProviderSupportsShouldProcess { get; }
    }



}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Rename", "Item", SupportsShouldProcess = true)]
    public class RenameItemCommand : CoreCommandWithPathsBase
    {
        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string NewName { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        // TODO: support for #DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime;
            runtime.PassThru = PassThru.IsPresent;
            InvokeProvider.Item.Rename(InternalPaths, NewName, runtime);
        }
    }



}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Remove", "Item", SupportsShouldProcess = true, DefaultParameterSetName = "Path")]
    public class RemoveItemCommand : CoreCommandWithFilteredPathsBase
    {
        // TODO: support for DynamicParameters (calling the providers appropriate method)

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Remove(InternalPaths, Recurse.IsPresent, ProviderRuntime);
        }
    }
}

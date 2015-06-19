// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute("Copy", "Item", DefaultParameterSetName="Path", SupportsShouldProcess=true
        /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113292"*/)]
    public class CopyItemCommand : CoreCommandWithFilteredPathsBase
    {
        [Parameter]
        public SwitchParameter Container { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        public CopyItemCommand() : base()
        {
            Container = true; // Documentation states that this must explicitly disabled (yep, that's strange, but true)
        }

        // TODO: #DynamicParameter support

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime;
            runtime.PassThru = PassThru;
            var container = Container.IsPresent ? CopyContainers.CopyTargetContainer
                                                : CopyContainers.CopyChildrenOfTargetContainer;
            InvokeProvider.Item.Copy(InternalPaths, Destination, Recurse.IsPresent, container, runtime);
        }
    }
}

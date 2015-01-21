using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Item", SupportsShouldProcess=true, DefaultParameterSetName="Path"
            /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113395"*/)]
    public class SetItemCommand : CoreCommandWithPathsBase
    {
        protected override bool ProviderSupportsShouldProcess
        {
            get
            {
                // TODO: useful implementation based on _paths and the affected providers
                return false;
            }
        }

        [ParameterAttribute(Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public Object Value { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime; // assign to var because ProviderRuntime generates instances
            runtime.PassThru = PassThru.IsPresent;
            InvokeProvider.Item.Set(InternalPaths, Value, runtime);
        }
    }
}


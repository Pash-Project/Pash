using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Clear", "Item", DefaultParameterSetName="Path", SupportsShouldProcess=true
            /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113283" */)] 
    public class ClearItemCommand : CoreCommandWithPathsBase
    {
        protected override bool ProviderSupportsShouldProcess
        {
            get
            {
                // TODO: useful implementation based on _paths and the affected providers
                return false;
            }
        }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Clear(InternalPaths, ProviderRuntime);
        }
    }
}


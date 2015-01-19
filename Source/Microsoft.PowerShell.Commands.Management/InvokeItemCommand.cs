using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Invoke", "Item", DefaultParameterSetName="Path", SupportsShouldProcess=true
            /* , SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113345" */)]
    public class InvokeItemCommand : CoreCommandWithPathsBase
    {
        protected override bool ProviderSupportsShouldProcess {
            get
            {
                // TODO: useful implementation based on _paths and the affected providers
                return false;
            }
        }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Invoke(InternalPaths, ProviderRuntime);
        }
    }
}


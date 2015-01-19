using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Item", DefaultParameterSetName="Path"
            /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113319" */)]
    public class GetItemCommand : CoreCommandWithPathsBase
    {
        [Parameter]
        public override SwitchParameter Force { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Get(InternalPaths, ProviderRuntime);
        }
    }
}


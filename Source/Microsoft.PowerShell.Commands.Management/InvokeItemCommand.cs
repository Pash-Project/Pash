using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Invoke", "Item", DefaultParameterSetName="Path", SupportsShouldProcess=true
            /* , SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113345" */)]
    public class InvokeItemCommand : CoreCommandWithCredentialsBase
    {
        private string[] _paths;

        protected override bool ProviderSupportsShouldProcess {
            get
            {
                // TODO: useful implementation based on _paths and the affected providers
                return false;
            }
        }

        [Parameter(Position=0, ParameterSetName="Path", Mandatory=true, ValueFromPipeline=true,
            ValueFromPipelineByPropertyName=true)]
        public string[] Path
        {
            get { return _paths; }
            set { _paths = value; }
        }

        [Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false,
            ValueFromPipelineByPropertyName=true)]
        [Alias("PSPath")]
        public string[] LiteralPath
        {
            get { return _paths; }
            set
            {
                AvoidWildcardExpansion = true;
                _paths = value;
            }
        }

        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Invoke(_paths, ProviderRuntime);
        }
    }
}


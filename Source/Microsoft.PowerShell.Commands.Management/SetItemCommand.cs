using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Item", SupportsShouldProcess=true, DefaultParameterSetName="Path"
            /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113395"*/)]
    public class SetItemCommand : CoreCommandWithCredentialsBase
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

        [ParameterAttribute(Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public Object Value { get; set; }

        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime; // assign to var because ProviderRuntime generates instances
            runtime.PassThru = PassThru.IsPresent;
            InvokeProvider.Item.Set(_paths, Value, runtime);
        }
    }
}


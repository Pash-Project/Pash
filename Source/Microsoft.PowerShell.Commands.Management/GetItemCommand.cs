using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Item", DefaultParameterSetName="Path"
            /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113319" */)]
    public class GetItemCommand : CoreCommandWithCredentialsBase
    {
        private string[] _paths;

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
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Get(_paths, ProviderRuntime);
        }
    }
}


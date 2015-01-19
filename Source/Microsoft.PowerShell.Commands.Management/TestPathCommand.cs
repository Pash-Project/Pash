using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Test", "Path", DefaultParameterSetName="Path" /*, SupportsTransactions=true
            , HelpUri="http://go.microsoft.com/fwlink/?LinkID=113418" */)]
    public class TestPathCommand : CoreCommandWithCredentialsBase
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
        public override string[] Include { get; set; }

        [Alias("Type")]
        [Parameter]
        public TestPathType PathType { get; set; }

        [Parameter]
        public SwitchParameter IsValid { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime;
            foreach (var curPath in _paths)
            {
                if (IsValid.IsPresent)
                {
                    WriteObject(SessionState.Path.IsValid(curPath, runtime));
                    continue;
                }

                throw new NotImplementedException("Only syntax checks for paths are already implemented");
            }
        }
    }
}


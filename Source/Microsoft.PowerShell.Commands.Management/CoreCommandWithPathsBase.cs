using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class CoreCommandWithPathsBase : CoreCommandWithCredentialsBase
    {
        protected string[] InternalPaths;

        [Parameter(Position=0, ParameterSetName="Path", Mandatory=true, ValueFromPipeline=true,
                   ValueFromPipelineByPropertyName=true)]
        public string[] Path
        {
            get { return InternalPaths; }
            set { InternalPaths = value; }
        }

        [Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false,
                   ValueFromPipelineByPropertyName=true)]
        [Alias("PSPath")]
        public string[] LiteralPath
        {
            get { return InternalPaths; }
            set
            {
                AvoidWildcardExpansion = true;
                InternalPaths = value;
            }
        }

        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override string[] Include { get; set; }
    }
}


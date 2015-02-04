using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /*
     * Many cmdlets support two parameter sets that are identical in all parameters, except for Path/LiteralPath.
     * Using LiteralPath only means that it may contain wildcards and should *not* be expanded/globbed.
     * While this is somehow pretty complicated (I guess a Switchparameter would have achieved the same), I try
     * to make the logic behind it as easy as possible. Simply derive from this class and use the protected InternalPaths
     * property instead of Path or LiteralPath. The runtime will have a flag that wildcards shouldn't be expanded.
     * The PathGlobber will consider this flag and behave appropriately.
     */
    public class CoreCommandWithPathsBase : CoreCommandWithCredentialsBase
    {
        protected string[] InternalPaths;

        protected override bool ProviderSupportsShouldProcess
        {
            get
            {
                // TODO: useful implementation based on InternalPaths and the affected providers
                return false;
            }
        }

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
    }
}


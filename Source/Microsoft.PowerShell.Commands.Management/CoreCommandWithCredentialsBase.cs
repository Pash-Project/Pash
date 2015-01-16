using System;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    public class CoreCommandWithCredentialsBase : CoreCommandBase
    {
        [Credential]
        [Parameter(ValueFromPipelineByPropertyName=true)]
        public PSCredential Credential { get; set; }

        protected CoreCommandWithCredentialsBase()
        {
        }

        internal virtual ProviderRuntime ProviderRuntime
        {
            get
            {
                var runtime = base.ProviderRuntime;
                runtime.Credential = Credential;
                return runtime;
            }
        }
    }
}


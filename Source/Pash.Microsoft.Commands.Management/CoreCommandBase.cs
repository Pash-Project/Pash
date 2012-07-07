using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class CoreCommandBase : PSCmdlet, IDynamicParameters, ISupportShouldProcess
    {
        public virtual string[] Include { get; set; }
        public virtual string[] Exclude { get; set; }
        public virtual string Filter { get; set; }
        public virtual SwitchParameter Force { get; set; }
        public bool SupportsShouldProcess { get; protected set; }

        protected CoreCommandBase()
        {
            SupportsShouldProcess = true;
        }

        protected bool DoesProviderSupportShouldProcess(string[] paths)
        {
            throw new NotImplementedException();
        }

        public object GetDynamicParameters()
        {
            throw new NotImplementedException();
        }

        internal ProviderRuntime ProviderRuntime
        {
            get
            {
                ProviderRuntime providerRuntime = new ProviderRuntime(this);
                // TODO: set the Force, Filter, Include, Exlude on the runtime
                return providerRuntime;
            }
        }
    }
}
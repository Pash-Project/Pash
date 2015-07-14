// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using Pash.Implementation;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Commands
{
    public abstract class CoreCommandBase : PSCmdlet, IDynamicParameters, ISupportShouldProcess
    {
        public virtual string[] Include { get; set; }
        public virtual string[] Exclude { get; set; }
        public virtual string Filter { get; set; }
        public virtual SwitchParameter Force { get; set; }
        protected virtual bool ProviderSupportsShouldProcess { get { return true; } }

        // The internal parameters cannot be used in that override-if-you-need fashion, because it's not part of the
        // public API
        internal bool AvoidWildcardExpansion { get; set; }

        public bool SupportsShouldProcess { get { return ProviderSupportsShouldProcess; } }

        protected bool DoesProviderSupportShouldProcess(string[] paths)
        {
            throw new NotImplementedException();
        }

        public virtual object GetDynamicParameters()
        {
            // TODO: For #DynamicParameter support, throw exception and force subclasses to override. So it's not forgotten
            return null;
        }

        internal virtual ProviderRuntime ProviderRuntime
        {
            get
            {
                var runtime = new ProviderRuntime(this);
                runtime.Include = Include == null ? new Collection<string>() : new Collection<string>(Include.ToList());
                runtime.Exclude = Exclude == null ? new Collection<string>() : new Collection<string>(Exclude.ToList());
                runtime.Filter = Filter;
                runtime.Force = Force;
                runtime.AvoidGlobbing = AvoidWildcardExpansion;
                return runtime;
            }
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    public class PassThroughContentCommandBase : ContentCommandBase
    {
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        internal override ProviderRuntime ProviderRuntime
        {
            get
            {
                var runtime = base.ProviderRuntime;
                runtime.PassThru = PassThru.IsPresent;
                return runtime;
            }
        }
    }
}

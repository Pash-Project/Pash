// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

//TODO: Make better

namespace Microsoft.PowerShell.Commands
{
    public abstract class ProviderCommandBase : PSCmdlet
    {
        [Parameter]
        public virtual string[] Exclude { get; set; }

        [Parameter]
        public virtual string Filter { get; set; }

        [Parameter]
        public virtual SwitchParameter Force { get; set; }

        [Parameter]
        public virtual string[] Include { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public PSCredential Credential { get; set; }
    }
}

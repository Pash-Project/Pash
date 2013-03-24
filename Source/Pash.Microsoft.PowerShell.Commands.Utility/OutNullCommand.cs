// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Null", SupportsShouldProcess = false)]
    public class OutNullCommand : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord() { }
    }
}

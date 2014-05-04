// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Null", SupportsShouldProcess = false)]
    public class OutNullCommand : OutCommandBase
    {
        protected override void ProcessRecord()
        {
            // do nothing
        }
    }
}

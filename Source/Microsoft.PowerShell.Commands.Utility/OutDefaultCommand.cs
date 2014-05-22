// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Default")]
    public class OutDefaultCommand : OutCommandBase
    {
        protected override void BeginProcessing()
        {
            OutputWriter = new HostOutputWriter(Host);
        }
    }
}

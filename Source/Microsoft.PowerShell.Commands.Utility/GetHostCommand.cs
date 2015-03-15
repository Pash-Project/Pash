// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Host")]
    [OutputType(typeof(PSHost))]
    public class GetHostCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            WriteObject(base.Host);
        }
    }
}

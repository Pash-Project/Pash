// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Host")]
    public class GetHostCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            WriteObject(base.Host);
        }
    }
}

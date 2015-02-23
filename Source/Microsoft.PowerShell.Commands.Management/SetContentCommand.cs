// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Microsoft.Commands.Management
{
    [CmdletAttribute(VerbsCommon.Set, "Content", DefaultParameterSetName = "Path" /* HelpUri = "http://go.microsoft.com/fwlink/?LinkID=113392"*/)]
    public class SetContentCommand : WriteContentCommandBase
    {
        protected override void ProcessRecord()
        {
            InvokeProvider.Content.Clear(InternalPaths, ProviderRuntime);
            WriteValues(InternalPaths);
        }
    }
}

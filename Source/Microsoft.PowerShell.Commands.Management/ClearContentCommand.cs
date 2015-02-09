// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Microsoft.Commands.Management
{
    [Cmdlet(VerbsCommon.Clear, "Content", DefaultParameterSetName="Path" /*HelpUri="http://go.microsoft.com/fwlink/?LinkID=113282" */)]
    public class ClearContentCommand : ContentCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (string path in Path)
            {
                InvokeProvider.Content.Clear(path);
            }
        }
    }
}

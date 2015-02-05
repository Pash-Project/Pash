// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Microsoft.Commands.Management
{
    [CmdletAttribute(VerbsCommon.Add, "Content", DefaultParameterSetName = "Path" /* HelpUri = "http://go.microsoft.com/fwlink/?LinkID=113278"*/)]
    public class AddContentCommand : WriteContentCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (string path in Path)
            {
                WriteValues(path, true);
            }
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using System.Management.Automation.Provider;
using System.IO;

namespace Microsoft.Commands.Management
{
    [CmdletAttribute(VerbsCommon.Add, "Content", DefaultParameterSetName = "Path" /* HelpUri = "http://go.microsoft.com/fwlink/?LinkID=113278"*/)]
    public class AddContentCommand : WriteContentCommandBase
    {
        protected override void BeginProcessing()
        {
            Writers = InvokeProvider.Content.GetWriter(InternalPaths, ProviderRuntime);

            foreach (IContentWriter writer in Writers)
            {
                writer.Seek(0, SeekOrigin.End);
            }
        }
    }
}

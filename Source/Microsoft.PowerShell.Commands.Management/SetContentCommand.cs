// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using Microsoft.PowerShell.Commands;

namespace Microsoft.Commands.Management
{
    [CmdletAttribute(VerbsCommon.Set, "Content", DefaultParameterSetName = "Path" /* HelpUri = "http://go.microsoft.com/fwlink/?LinkID=113392"*/)]
    public class SetContentCommand : WriteContentCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (string path in Path)
            {
                InvokeProvider.Content.Clear(path);
                WriteValues(path);
            }
        }

        private void WriteValues(string path)
        {
            IContentWriter writer = InvokeProvider.Content.GetWriter(path).Single();

            try
            {
                writer.Write(Value);
            }
            finally
            {
                writer.Close();
            }
        }
    }
}

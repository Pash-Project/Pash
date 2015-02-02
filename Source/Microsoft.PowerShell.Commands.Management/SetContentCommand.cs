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
                WriteValues(path);
            }
        }

        private void WriteValues(string path)
        {
            // Default file encoding is ASCII.
            using (var writer = new StreamWriter(path, false, Encoding.ASCII))
            {
                foreach (object obj in Value)
                {
                    writer.WriteLine(obj);
                }
            }
        }
    }
}

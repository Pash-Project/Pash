// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class FileSystemContentReaderDynamicParameters : FileSystemContentDynamicParametersBase
    {
        [Parameter]
        public string Delimiter { get; set; }
        public bool DelimiterSpecified { get; private set; }
        [Parameter]
        public SwitchParameter Wait { get; set; }

        public FileSystemContentReaderDynamicParameters()
        {
            Delimiter = "\n";
        }
    }
}

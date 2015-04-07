// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
    public class WriteContentCommandBase : PassThroughContentCommandBase
    {
        [AllowNull]
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [AllowEmptyCollection]
        public object[] Value { get; set; }

        internal Collection<IContentWriter> Writers { get; set; }

        protected override void ProcessRecord()
        {
            WriteValues();
        }

        internal void WriteValues()
        {
            foreach (IContentWriter writer in Writers)
            {
                writer.Write(Value);

                if (PassThru.IsPresent)
                {
                    WriteObject(Value, true);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (Writers != null)
            {
                foreach (IContentWriter writer in Writers)
                {
                    writer.Close();
                }
            }
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        internal void WriteValues(string[] path, bool seekEnd = false)
        {
            var writers = InvokeProvider.Content.GetWriter(path, ProviderRuntime);

            foreach (var writer in writers)
            {
                try
                {
                    if (seekEnd)
                    {
                        writer.Seek(0, SeekOrigin.End);
                    }
                    writer.Write(Value);

                    if (PassThru.IsPresent)
                    {
                        WriteObject(Value, true);
                    }
                }
                finally
                {
                    writer.Close();
                }
            }
        }
    }
}

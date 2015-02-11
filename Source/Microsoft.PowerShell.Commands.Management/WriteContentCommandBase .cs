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

        internal void WriteValues(string path, bool seekEnd = false)
        {
            IContentWriter writer = InvokeProvider.Content.GetWriter(path).Single();

            try
            {
                if (seekEnd)
                {
                    writer.Seek(0, SeekOrigin.End);
                }
                writer.Write(Value);
            }
            finally
            {
                writer.Close();
            }
        }
    }
}

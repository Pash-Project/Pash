using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class TableFormatProcessor : FormatProcessor
    {
        internal TableFormatProcessor(OutputWriter writer) : base(FormatShape.Table, writer)
        {
        }

        protected override void ProcessFormatEntry (FormatEntryData data)
        {
            if (String.IsNullOrEmpty(data.Data))
            {
                return;
            }
            if (data.WriteToErrorStream)
            {
                OutputWriter.WriteErrorLine(data.Data);
            }
            else
            {
                OutputWriter.WriteLine(data.Data);
            }
        }
    }
}


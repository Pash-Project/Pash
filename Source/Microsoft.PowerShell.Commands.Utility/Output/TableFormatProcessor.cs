using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell.Commands.Utility
{

    internal class TableFormatProcessor : FormatProcessor
    {
        internal TableFormatProcessor(OutputWriter writer) : base(FormatShape.Table, writer)
        {
        }

        protected override void ProcessGroupStart(GroupStartData data)
        {
        }

        protected override void ProcessSimpleFormatEntry(SimpleFormatEntryData data)
        {
            OutputWriter.WriteToErrorStream = data.WriteToErrorStream;
            if (!String.IsNullOrEmpty(data.Value))
            {
                OutputWriter.WriteLine(data.Value);
            }
        }

        protected override void ProcessObjectFormatEntry(FormatEntryData data)
        {

        }
    }
}


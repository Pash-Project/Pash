using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatProcessor : FormatProcessor
    {
        internal ListFormatProcessor(OutputWriter writer) : base(FormatShape.List, writer)
        {
        }

        protected override void ProcessObjectFormatEntry(FormatEntryData data)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessSimpleFormatEntry(SimpleFormatEntryData data)
        {
            throw new NotImplementedException();
        }
    }
}


using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatProcessor : FormatProcessor
    {
        internal ListFormatProcessor(OutputWriter writer) : base(FormatShape.List, writer)
        {
        }

        protected override void ProcessFormatEntry (FormatEntryData data)
        {
            throw new NotImplementedException ();
        }
    }
}


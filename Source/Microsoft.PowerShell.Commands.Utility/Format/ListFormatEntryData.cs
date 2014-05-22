using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatEntryData : FormatEntryData
    {
        public List<FormatObjectProperty> Entries { get; set; }

        internal ListFormatEntryData(List<FormatObjectProperty> entries) : base(FormatShape.List)
        {
            Entries = entries;
        }
    }
}


using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class FormatEntryData : FormatData
    {
        internal bool WriteToErrorStream { get; set; }

        internal FormatEntryData(FormatShape shape) : base(shape)
        {
        }

        internal FormatEntryData(FormatEntryData entry) : base(entry.Shape)
        {
            WriteToErrorStream = entry.WriteToErrorStream;
        }
    }
}


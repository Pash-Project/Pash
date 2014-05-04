using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class FormatEntryData : FormatData
    {
        internal bool WriteToErrorStream { get; set; }

        internal string Data { get; set; }

        internal FormatEntryData(FormatShape shape, string data) : base(shape)
        {
            Data = data;
        }
    }
}


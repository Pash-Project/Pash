using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class SimpleFormatEntryData : FormatEntryData
    {
        public string Value { get; set; }

        internal SimpleFormatEntryData(FormatShape shape, string value) : base(shape)
        {
            Value = value;
        }
    }
}


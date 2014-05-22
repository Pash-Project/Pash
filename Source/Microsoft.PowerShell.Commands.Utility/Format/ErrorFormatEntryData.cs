using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ErrorFormatEntryData : FormatEntryData
    {
        public string Message { get; private set; }
        public string CategoryInfo { get; private set; }
        public string FullyQualifiedErrorId { get; private set; }

        internal ErrorFormatEntryData(FormatShape shape, string message, string categoryInfo, string errorId)
            : base(shape)
        {
            Message = message;
            CategoryInfo = categoryInfo;
            FullyQualifiedErrorId = errorId;
        }
    }
}


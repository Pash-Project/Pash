using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Flags]
    internal enum FormattingState
    {
        FormatStart,
        GroupStart,
        GroupEnd,
        FormatEnd
    }
}


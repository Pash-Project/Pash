using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class FormatData
    {
        internal FormatShape Shape { get; private set; }

        internal FormatData(FormatShape shape)
        {
            Shape = shape;
        }
    }
}


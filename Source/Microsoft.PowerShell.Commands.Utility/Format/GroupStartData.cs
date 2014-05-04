using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class GroupStartData : FormatData
    {
        public string GroupName { get; set; }
        internal GroupStartData(FormatShape shape) : base(shape)
        {
        }
    }
}


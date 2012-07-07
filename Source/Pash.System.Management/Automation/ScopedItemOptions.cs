using System;

namespace System.Management.Automation
{
    [Flags]
    public enum ScopedItemOptions
    {
        None = 0,
        ReadOnly = 1,
        Constant = 2,
        Private = 4,
        AllScope = 8,
    }
}

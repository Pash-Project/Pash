using System;

namespace System.Management.Automation
{
    [Flags]
    public enum WildcardOptions
    {
        None = 0,
        Compiled = 1,
        IgnoreCase = 2,
    }
}

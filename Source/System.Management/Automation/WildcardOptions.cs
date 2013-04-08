// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

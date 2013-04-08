// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    [Flags]
    public enum SwitchFlags
    {
        // TODO: rewrite in hex.
        None = 0,
        File = 1,
        Regex = 2,
        Wildcard = 4,
        Exact = 8,
        CaseSensitive = 16,
        Parallel = 32,
    }
}

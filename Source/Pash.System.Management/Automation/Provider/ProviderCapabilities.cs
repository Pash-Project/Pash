// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Provider
{
    [Flags]
    public enum ProviderCapabilities
    {
        None = 0,
        Include = 1,
        Exclude = 2,
        Filter = 4,
        ExpandWildcards = 8,
        ShouldProcess = 16,
        Credentials = 32,
    }
}

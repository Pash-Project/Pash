// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    [Flags]
    public enum PSCredentialTypes
    {
        Generic = 1,
        Domain = 2,
        Default = 3,
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    [Flags]
    public enum ShouldProcessReason
    {
        None = 0,
        WhatIf = 1,
    }
}

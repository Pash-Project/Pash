// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Host
{
    [Flags]
    public enum ReadKeyOptions
    {
        AllowCtrlC = 1,
        NoEcho = 2,
        IncludeKeyDown = 4,
        IncludeKeyUp = 8,
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Host
{
    [Flags]
    public enum ControlKeyStates
    {
        RightAltPressed = 1,
        LeftAltPressed = 2,
        RightCtrlPressed = 4,
        LeftCtrlPressed = 8,
        ShiftPressed = 16,
        NumLockOn = 32,
        ScrollLockOn = 64,
        CapsLockOn = 128,
        EnhancedKey = 256,
    }
}

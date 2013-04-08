// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

namespace System.Management.Automation
{
    /// <summary>
    /// Options to take if an error occurs.
    /// </summary>
    public enum ActionPreference
    {
        SilentlyContinue,
        Stop,
        Continue,
        Inquire
    }
}


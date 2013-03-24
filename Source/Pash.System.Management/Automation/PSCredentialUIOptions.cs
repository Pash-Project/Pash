// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    [Flags]
    public enum PSCredentialUIOptions
    {
        None = 0,
        Default = 1,
        ValidateUserNameSyntax = 1,
        AlwaysPrompt = 2,
        ReadOnlyUserName = 3,
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    /// <summary>
    /// Used by the Command Manager to categories the different things that map to a parser command.
    /// </summary>
    [Flags]
    public enum CommandTypes
    {
        Alias = 1,
        Function = 2,
        Filter = 4,
        Cmdlet = 8,
        ExternalScript = 16,
        Application = 32,
        Script = 64,
        All = 127,
    }
}

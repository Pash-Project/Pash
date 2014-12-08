// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation
{
    public enum SessionStateCategory
    {
        Variable,
        Alias,
        Function,
        Filter,
        Drive,
        CmdletProvider,
        Cmdlet,
        Module // specification says that this shouldn't be here. but we need it here, so nevermind
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation.Runspaces;

namespace Pash.Implementation
{
    public class LocalRunspaceConfiguration : RunspaceConfiguration
    {
        public override string ShellId
        {
            get
            {
                return "LocalRunspaceConfiguration";
            }
        }
    }
}

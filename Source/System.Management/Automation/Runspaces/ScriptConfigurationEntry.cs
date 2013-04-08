// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class ScriptConfigurationEntry : RunspaceConfigurationEntry
    {
        public ScriptConfigurationEntry(string name, string definition)
            : base(name)
        {
            Definition = definition;
        }

        public string Definition { get; private set; }
    }
}

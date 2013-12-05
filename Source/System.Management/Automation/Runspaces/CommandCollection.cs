// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
    public sealed class CommandCollection : Collection<Command>
    {
        public void Add(string command)
        {
            Add(new Command(command));
        }

        public void AddScript(string scriptContents)
        {
            AddScript(scriptContents, true); //per default a script runs in  its own scope
        }

        public void AddScript(string scriptContents, bool useLocalScope)
        {
            Add(new Command(scriptContents, true, useLocalScope));
        }

        // internals
        internal CommandCollection()
        {
        }
    }
}

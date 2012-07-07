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
            Add(new Command(scriptContents, true));
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

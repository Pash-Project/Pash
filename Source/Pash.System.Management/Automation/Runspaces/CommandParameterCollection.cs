using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
    public sealed class CommandParameterCollection : Collection<CommandParameter>
    {
        // internals
        internal CommandParameterCollection()
        {
        }

        public void Add(string name)
        {
            Add(new CommandParameter(name));
        }

        public void Add(string name, object value)
        {
            Add(new CommandParameter(name, value));
        }
    }
}

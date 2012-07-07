using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class CommandParameter
    {
        public CommandParameter(string name)
        {
            Name = name;
        }
        public CommandParameter(string name, object value) 
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public object Value { get; private set; }
    }
}

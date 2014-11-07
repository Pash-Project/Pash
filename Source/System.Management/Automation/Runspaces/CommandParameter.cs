// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class CommandParameter
    {
        public CommandParameter(string name) : this(name, null, false)
        {
        }

        public CommandParameter(string name, object value) : this(name, value, false)
        {
        }

        internal CommandParameter(string name, object value, bool requiresValue)
        {
            Name = name;
            Value = value;
            HasExplicitArgument = requiresValue;
        }

        public string Name { get; private set; }
        public object Value { get; private set; }
        internal bool HasExplicitArgument { get; private set; }

        public override string ToString()
        {
            return Name + "=" + Value;
        }
    }
}

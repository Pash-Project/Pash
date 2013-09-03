// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateVariableEntry : ConstrainedSessionStateEntry
    {
        public object Value { get; private set; }
        public string Description { get; private set; }
        public ScopedItemOptions Options { get; private set; }
        public Collection<Attribute> Attributes { get; private set; }

        public SessionStateVariableEntry(string name, object value, string description)
            : this(name, value, description, ScopedItemOptions.None)
        {
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options)
            : this(name, value, description, options, new Collection<Attribute>())
        {
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Collection<Attribute> attributes)
            : base(name, SessionStateEntryVisibility.Public)
        {
            this.Options = options;
            this.Description = description;
            this.Value = value;
            this.Attributes = attributes;
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Attribute attribute)
            : this(name, value, description, options)
        {
            this.Attributes.Add(attribute);
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

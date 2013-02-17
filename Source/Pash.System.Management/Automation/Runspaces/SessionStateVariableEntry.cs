// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateVariableEntry : ConstrainedSessionStateEntry
    {
        public object Value
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public ScopedItemOptions Options
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Collection<Attribute> Attributes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SessionStateVariableEntry(string name, object value, string description)
            : base(name, SessionStateEntryVisibility.Public)
        {
            throw new NotImplementedException();
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options)
            : base(name, SessionStateEntryVisibility.Public)
        {
            throw new NotImplementedException();
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Collection<Attribute> attributes)
            : base(name, SessionStateEntryVisibility.Public)
        {
            throw new NotImplementedException();
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Attribute attribute)
            : base(name, SessionStateEntryVisibility.Public)
        {
            throw new NotImplementedException();
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

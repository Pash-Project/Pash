// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateFunctionEntry : SessionStateCommandEntry
    {
        public string Definition
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

        public SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options)
            : base(name, SessionStateEntryVisibility.Public)
        {
            throw new NotImplementedException();
        }

        public SessionStateFunctionEntry(string name, string definition)
            : base(name)
        {
            throw new NotImplementedException();
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

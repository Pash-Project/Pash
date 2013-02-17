// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public abstract class SessionStateCommandEntry : ConstrainedSessionStateEntry
    {
        public CommandTypes CommandType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected SessionStateCommandEntry(string name)
            : base(name, SessionStateEntryVisibility.Public)
        {
            throw new NotImplementedException();
        }

        protected internal SessionStateCommandEntry(string name, SessionStateEntryVisibility visibility)
            : base(name, visibility)
        {
        }

    }
}

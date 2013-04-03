// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public abstract class SessionStateCommandEntry : ConstrainedSessionStateEntry
    {
        internal CommandTypes commandType;

        public CommandTypes CommandType
        {
            get
            {
                return commandType;
            }
        }

        protected SessionStateCommandEntry(string name)
            : base(name, SessionStateEntryVisibility.Public)
        {

        }

        protected internal SessionStateCommandEntry(string name, SessionStateEntryVisibility visibility)
            : base(name, visibility)
        {
        }

    }
}

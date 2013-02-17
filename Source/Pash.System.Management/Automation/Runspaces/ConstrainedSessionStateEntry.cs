// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public abstract class ConstrainedSessionStateEntry : InitialSessionStateEntry
    {
        private SessionStateEntryVisibility visibility;

        public SessionStateEntryVisibility Visibility
        {
            get
            {
                return this.visibility;
            }
            set
            {
                this.visibility = value;
            }
        }

        protected ConstrainedSessionStateEntry(string name, SessionStateEntryVisibility visibility)
            : base(name)
        {
            this.visibility = visibility;
        }
    }
}

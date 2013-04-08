// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateAliasEntry : SessionStateCommandEntry
    {
        private string definition;
        private string description;
        ScopedItemOptions scopeoptions;

        public string Definition
        {
            get
            {
                return definition;
            }
        }
        public string Description
        {
            get
            {
                return description;
            }
        }
        public ScopedItemOptions Options
        {
            get
            {
                return scopeoptions;
            }
        }

        public SessionStateAliasEntry(string name, string definition)
            : base(name, SessionStateEntryVisibility.Public)
        {
            this.definition = definition;
        }

        public SessionStateAliasEntry(string name, string definition, string description)
            : base(name, SessionStateEntryVisibility.Public)
        {
            this.definition = definition;
            this.description = description;
        }

        public SessionStateAliasEntry(string name, string definition, string description, ScopedItemOptions options)
            : base(name, SessionStateEntryVisibility.Public)
        {
            this.definition = definition;
            this.description = description;
            this.scopeoptions = options;
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

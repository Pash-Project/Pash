// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateFunctionEntry : SessionStateCommandEntry
    {
        private string definition;
        private ScopedItemOptions options;

        public string Definition
        {
            get
            {
                return definition;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                return options;
            }
        }

        public SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options)
            : base(name, SessionStateEntryVisibility.Public)
        {
            this.commandType = CommandTypes.Function;
            this.definition = definition;
            this.options = options;
        }

        public SessionStateFunctionEntry(string name, string definition)
            : base(name)
        {
            this.commandType = CommandTypes.Function;
            this.definition = definition;
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

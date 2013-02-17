// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateCmdletEntry : SessionStateCommandEntry
    {
        public Type ImplementingType
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public string HelpFileName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SessionStateCmdletEntry(string name, Type implementingType, string helpFileName)
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

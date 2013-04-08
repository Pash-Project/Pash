// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateCmdletEntry : SessionStateCommandEntry
    {
        Type implentingtype;
        string helpfilename;

        public Type ImplementingType
        {
            get
            {
                return implentingtype;
            }
        }
        public string HelpFileName
        {
            get
            {
                return helpfilename;
            }
        }

        public SessionStateCmdletEntry(string name, Type implementingType, string helpFileName)
            : base(name, SessionStateEntryVisibility.Public)
        {
            this.implentingtype = implementingType;
            this.helpfilename = helpFileName;
            this.commandType = CommandTypes.Cmdlet;
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

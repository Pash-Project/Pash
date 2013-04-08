// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateScriptEntry : SessionStateCommandEntry
    {
        private string path;

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public SessionStateScriptEntry(string path)
            : base(path, SessionStateEntryVisibility.Public)
        {
            this.path = path;
            this.commandType = CommandTypes.ExternalScript;
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

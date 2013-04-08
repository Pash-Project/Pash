// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateApplicationEntry : SessionStateCommandEntry
    {
        private string path;

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public SessionStateApplicationEntry(string path)
            : base(path, SessionStateEntryVisibility.Public)
        {
            this.path = path;
            this.commandType = CommandTypes.Application;
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

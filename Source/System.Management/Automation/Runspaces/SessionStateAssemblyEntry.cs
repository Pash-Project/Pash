// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateAssemblyEntry : InitialSessionStateEntry
    {
        private string fileName;

        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        public SessionStateAssemblyEntry(string name, string fileName)
            : base(name)
        {
            this.fileName = fileName;
        }

        public SessionStateAssemblyEntry(string name)
            : base(name)
        {
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateTypeEntry : InitialSessionStateEntry
    {
        public SessionStateTypeEntry(string fileName)
            : base("*")
        {
            throw new NotImplementedException();
        }

        public SessionStateTypeEntry(TypeTable typeTable)
            : base("*")
        {
            throw new NotImplementedException();
        }

        public override InitialSessionStateEntry Clone()
        {
            throw new NotImplementedException();
        }
    }
}

// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class SessionStateFormatEntry : InitialSessionStateEntry
    {
        public string FileName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FormatTable Formattable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SessionStateFormatEntry(string fileName)
            : base("*")
        {
            throw new NotImplementedException();
        }

        public SessionStateFormatEntry(FormatTable formattable)
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

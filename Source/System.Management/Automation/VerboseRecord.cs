// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class VerboseRecord : InformationalRecord
    {
        internal VerboseRecord(string message)
            : base(message)
        {
        }
        internal VerboseRecord(PSObject psObject)
            : base(psObject)
        {
        }
    }
}

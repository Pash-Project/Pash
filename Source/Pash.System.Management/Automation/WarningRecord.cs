// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class WarningRecord : InformationalRecord
    {
        internal WarningRecord(string message)
            : base(message)
        {
        }
        internal WarningRecord(PSObject psObject)
            : base(psObject)
        {
        }
    }
}

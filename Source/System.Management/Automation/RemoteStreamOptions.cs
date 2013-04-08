// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    [Flags]
    public enum RemoteStreamOptions
    {
        AddInvocationInfoToErrorRecord = 1,
        AddInvocationInfoToWarningRecord = 2,
        AddInvocationInfoToDebugRecord = 4,
        AddInvocationInfoToVerboseRecord = 8,
        AddInvocationInfo = 15
    }
}

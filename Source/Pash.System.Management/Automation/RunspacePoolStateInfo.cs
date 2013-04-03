// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    public sealed class RunspacePoolStateInfo
    {
        public RunspacePoolState State
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Exception Reason
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public RunspacePoolStateInfo(RunspacePoolState state, Exception reason)
        {
            throw new NotImplementedException();
        }
    }
}

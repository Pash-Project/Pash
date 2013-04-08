// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public sealed class RunspacePoolStateChangedEventArgs : EventArgs
    {
        public RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal RunspacePoolStateChangedEventArgs(RunspacePoolState state)
        {
            throw new NotImplementedException();
        }

        internal RunspacePoolStateChangedEventArgs(RunspacePoolStateInfo stateInfo)
        {
            throw new NotImplementedException();
        }
    }
}

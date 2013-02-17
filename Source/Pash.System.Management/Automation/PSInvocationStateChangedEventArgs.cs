// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class PSInvocationStateChangedEventArgs : EventArgs
    {
        public PSInvocationStateInfo InvocationStateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal PSInvocationStateChangedEventArgs(PSInvocationStateInfo psStateInfo)
        {
            throw new NotImplementedException();
        }
    }
}

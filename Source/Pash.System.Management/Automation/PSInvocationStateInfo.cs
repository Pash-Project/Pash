// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    public sealed class PSInvocationStateInfo
    {
        public PSInvocationState State
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
        internal PSInvocationStateInfo(PSInvocationState state, Exception reason)
        {
            throw new NotImplementedException();
        }
        internal PSInvocationStateInfo(PipelineStateInfo pipelineStateInfo)
        {
            throw new NotImplementedException();
        }
        internal PSInvocationStateInfo Clone()
        {
            throw new NotImplementedException();
        }
    }
}

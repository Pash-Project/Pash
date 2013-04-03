// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class DataAddedEventArgs : EventArgs
    {
        public int Index
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Guid PowerShellInstanceId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        internal DataAddedEventArgs(Guid psInstanceId, int index)
        {
            throw new NotImplementedException();
        }
    }
}

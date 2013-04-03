// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    [Serializable]
    public class PSSnapInException : RuntimeException
    {

        public PSSnapInException()
        {
        }

        public PSSnapInException(string message)
            : base(message)
        {
        }

        public PSSnapInException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

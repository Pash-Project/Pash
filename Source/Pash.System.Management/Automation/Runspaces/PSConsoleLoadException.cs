// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    [Serializable]
    public class PSConsoleLoadException : SystemException, IContainsErrorRecord
    {
        public PSConsoleLoadException()
        {
            throw new NotImplementedException();
        }

        public PSConsoleLoadException(string message)
            : base(message)
        {
            throw new NotImplementedException();
        }

        public PSConsoleLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
            throw new NotImplementedException();
        }

        public ErrorRecord ErrorRecord
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

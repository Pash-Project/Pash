// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Exception is thrown when they are circular references to an ErrorRecord.
    /// </summary>
    [Serializable]
    public class ParentContainsErrorRecordException : SystemException
    {
        public ParentContainsErrorRecordException()
        {
        }

        public ParentContainsErrorRecordException(Exception wrapperException) 
            : base(wrapperException.Message)
        {
        }

        public ParentContainsErrorRecordException(string message) 
            : base(message)
        {
        }

        public ParentContainsErrorRecordException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected ParentContainsErrorRecordException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


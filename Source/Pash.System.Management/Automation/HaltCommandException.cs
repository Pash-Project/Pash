// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Called when a cmdlet wants to terminate execution without a specific error.
    /// </summary>
    [Serializable]
    public class HaltCommandException : SystemException
    {
        public HaltCommandException()
        {
        }

        public HaltCommandException(string message)
            : base(message)
        {
        }

        public HaltCommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected HaltCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


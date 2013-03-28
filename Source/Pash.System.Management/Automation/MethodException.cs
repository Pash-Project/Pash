// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when Pash has a problem calling a method.
    /// </summary>
    [Serializable]
    public class MethodException : ExtendedTypeSystemException
    {

        public MethodException() 
            : base(typeof(MethodException).FullName)
        {
        }

        public MethodException(string message) 
            : base(message)
        {
        }

        public MethodException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


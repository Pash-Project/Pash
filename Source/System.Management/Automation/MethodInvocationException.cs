// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Called when Pash has a problem invoking a method.
    /// </summary>
    [Serializable]
    public class MethodInvocationException : MethodException
    {
        public MethodInvocationException()
            : base(typeof(MethodInvocationException).FullName)
        {
        }

        public MethodInvocationException(string message)
            : base(message)
        {
        }

        public MethodInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MethodInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


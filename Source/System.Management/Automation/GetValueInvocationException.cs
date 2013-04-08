// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// An error thown by Pash when invokes a member in an object along the pipeline.
    /// </summary>
    [Serializable]
    public class GetValueInvocationException : GetValueException
    {
        public GetValueInvocationException()
            : base(typeof(GetValueInvocationException).FullName)
        {
        }

        public GetValueInvocationException(string message)
            : base(message)
        {
        }

        protected GetValueInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GetValueInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


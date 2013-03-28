// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// An exception that is thrown when Pash has an error getting a member of an object along the pipeline.
    /// </summary>
    [Serializable]
    public class GetValueException : ExtendedTypeSystemException
    {
        public GetValueException() 
            : base(typeof(GetValueException).FullName)
        {
        }

        public GetValueException(string message) 
            : base(message)
        {
        }

        protected GetValueException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GetValueException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}


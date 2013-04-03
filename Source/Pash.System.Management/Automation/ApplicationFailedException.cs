// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Exception when a application fails for some reason.
    /// </summary>
    [Serializable]
    public class ApplicationFailedException : RuntimeException
    {
        public ApplicationFailedException()
        {
            base.Id = "NativeCommandFailed";
            base.Category = ErrorCategory.ResourceUnavailable;
        }

        public ApplicationFailedException(string message)
            : base(message)
        {
            base.Id = "NativeCommandFailed";
            base.Category = ErrorCategory.ResourceUnavailable;
        }

        public ApplicationFailedException(string message, Exception innerException) 
            : base(message, innerException)
        {
            base.Id = "NativeCommandFailed";
            base.Category = ErrorCategory.ResourceUnavailable;
        }

        protected ApplicationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


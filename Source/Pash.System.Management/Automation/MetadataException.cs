// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class MetadataException : RuntimeException
    {
        public MetadataException()
            : base(typeof(MetadataException).FullName)
        {
            base.Category = ErrorCategory.MetadataError;
        }

        public MetadataException(string message)
            : base(message)
        {
            base.Category = ErrorCategory.MetadataError;
        }

        protected MetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            base.Category = ErrorCategory.MetadataError;
        }

        public MetadataException(string message, Exception innerException)
            : base(message, innerException)
        {
            base.Category = ErrorCategory.MetadataError;
        }
    }
}


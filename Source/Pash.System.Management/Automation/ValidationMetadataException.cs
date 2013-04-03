// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
	/// <summary>
	/// Thrown when an validation attribute fails to validate.
	/// </summary>
    [Serializable]
    public class ValidationMetadataException : MetadataException
    {
        public ValidationMetadataException()
            : base(typeof(ValidationMetadataException).FullName)
        {
        }

        public ValidationMetadataException(string message)
            : base(message)
        {
        }

        protected ValidationMetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ValidationMetadataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}



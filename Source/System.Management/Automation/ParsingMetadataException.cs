// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when an error happens related to parsing metadata (attributes).
    /// </summary>
    [Serializable]
    public class ParsingMetadataException : MetadataException
    {
        public ParsingMetadataException()
            : base(typeof(ParsingMetadataException).FullName)
        {
        }

        public ParsingMetadataException(string message)
            : base(message)
        {
        }

        public ParsingMetadataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ParsingMetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


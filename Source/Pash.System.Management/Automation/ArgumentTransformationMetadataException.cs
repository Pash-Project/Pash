// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// When an arguement transformation errors, this exception is thrown.
    /// </summary>
    [Serializable]
    public class ArgumentTransformationMetadataException : MetadataException
    {
        public ArgumentTransformationMetadataException() 
            : base(typeof(ArgumentTransformationMetadataException).FullName)
        {
        }

        public ArgumentTransformationMetadataException(string message) 
            : base(message)
        {
        }

        protected ArgumentTransformationMetadataException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public ArgumentTransformationMetadataException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}


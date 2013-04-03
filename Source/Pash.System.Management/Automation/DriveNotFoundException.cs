// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when a drive is not found.
    /// </summary>
    [Serializable]
    public class DriveNotFoundException : SessionStateException
    {
        public DriveNotFoundException()
        {
        }

        public DriveNotFoundException(string message) 
            : base(message)
        {
        }

        public DriveNotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected DriveNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


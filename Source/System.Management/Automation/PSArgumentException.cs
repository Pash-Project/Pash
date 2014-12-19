// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when an arguement has an invalid parameter.
    /// </summary>
    [Serializable]
    public class PSArgumentException : ArgumentException, IContainsErrorRecord
    {
        public ErrorRecord ErrorRecord { get; private set; }

        public PSArgumentException()
            : this("Invalid Argument!")
        {
        }

        public PSArgumentException(string message, string paramName)
            : this(message + "Invalid parameter: " + paramName)
        {
        }

        public PSArgumentException(string message)
            : this(message, (Exception) null)
        {
        }

        internal PSArgumentException(string message, Exception innerException)
            : this(message, "Invalid Argument", ErrorCategory.InvalidArgument)
        {
        }

        internal PSArgumentException(string message, string errorId, ErrorCategory errorCat)
            : this(message, "Invalid Argument", ErrorCategory.InvalidArgument, null)
        {
        }

        internal PSArgumentException(string message, string errorId, ErrorCategory errorCat, Exception innerException)
            : base(message, innerException)
        {
            ErrorRecord = new ErrorRecord(this, errorId, errorCat, null);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        protected PSArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Irony.Parsing;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when there is an invalid operation detected by the parser.
    /// </summary>
    [Serializable]
    public class PSInvalidOperationException : InvalidOperationException, IContainsErrorRecord
    {
        public ErrorRecord ErrorRecord { get; private set; }
        internal bool Terminating { get; private set; }

        public PSInvalidOperationException()
            : this("Invalid Operation")
        {
        }

        public PSInvalidOperationException(string message)
            : this(message, null)
        {
        }

        public PSInvalidOperationException(string message, Exception innerException)
            : this(message, "InvalidOperation", ErrorCategory.InvalidOperation, innerException)
        {
        }


        internal PSInvalidOperationException(string message, string id, ErrorCategory errorCategory)
            : this(message, id, errorCategory, null)
        {
        }

        internal PSInvalidOperationException(string message, string id, ErrorCategory errorCategory,
                                             Exception innerException, bool terminating = true)
            : base(message, innerException)
        {
            Terminating = terminating;
            ErrorRecord = new ErrorRecord(this, id, errorCategory, null);
        }


        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

        }

        protected PSInvalidOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorRecord = new ErrorRecord(this, "InvalidOperation",
                                          ErrorCategory.InvalidOperation, null);
        }
    }
}


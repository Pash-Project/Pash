// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when there is an invalid operation detected by the parser.
    /// </summary>
    [Serializable]
    public class PSInvalidOperationException : InvalidOperationException, IContainsErrorRecord
    {
        private string id;
        private ErrorRecord error;

        public PSInvalidOperationException()
        {
            id = "InvalidOperation";
        }

        public PSInvalidOperationException(string message)
            : base(message)
        {
            id = "InvalidOperation";
        }

        public PSInvalidOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
            id = "InvalidOperation";
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            id = "InvalidOperation";
        }

        protected PSInvalidOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            id = "InvalidOperation";
        }

        public ErrorRecord ErrorRecord
        {
            get
            {
                if (error != null)
                {
                    return error;
                }
                error = new ErrorRecord(new ParentContainsErrorRecordException(this), id, ErrorCategory.InvalidOperation, null);
                return error;
            }
        }
    }
}


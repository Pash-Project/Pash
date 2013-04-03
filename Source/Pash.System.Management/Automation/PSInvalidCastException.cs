// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Management.Automation;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when there is an invalid cast (pretty much when) LanguagePrimitives.ConvertTo() fails.
    /// </summary>
    [Serializable]
    public class PSInvalidCastException : InvalidCastException, IContainsErrorRecord
    {
        private string id;
        private ErrorRecord error;

        public PSInvalidCastException()
            : base(typeof(PSInvalidCastException).FullName)
        {
            id = "PSInvalidCastException";
        }

        public PSInvalidCastException(string message)
            : base(message)
        {
            id = "PSInvalidCastException";
        }

        public PSInvalidCastException(string message, Exception innerException)
            : base(message, innerException)
        {
            id = "PSInvalidCastException";
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            id = "PSInvalidCastException";
        }

        protected PSInvalidCastException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            id = "PSInvalidCastException";
        }

        public ErrorRecord ErrorRecord
        {
            get
            {
                if (error != null)
                {
                    return error;
                }
                error = new ErrorRecord(new ParentContainsErrorRecordException(this), id, ErrorCategory.InvalidArgument, null);
                return error;
            }
        }
    }
}


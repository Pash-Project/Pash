// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Management.Automation;

namespace System.Management.Automation
{
    /// <summary>
    /// Called when an arguement is out of range, by the range validation attributes.
    /// </summary>
    [Serializable]
    public class PSArgumentOutOfRangeException : ArgumentOutOfRangeException, IContainsErrorRecord
    {
        internal string id;
        private ErrorRecord error;

        public PSArgumentOutOfRangeException()
        {
            id = "ArgumentOutOfRange";
        }

        public PSArgumentOutOfRangeException(string paramName)
            : base(paramName)
        {
            id = "ArgumentOutOfRange";
        }

        protected PSArgumentOutOfRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            id = "ArgumentOutOfRange";
        }

        public PSArgumentOutOfRangeException(string message, Exception innerException)
            : base(message, innerException)
        {
            id = "ArgumentOutOfRange";
        }

        public PSArgumentOutOfRangeException(string paramName, object actualValue, string message)
            : base(paramName, actualValue, message)
        {
            id = "ArgumentOutOfRange";
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            id = "ArgumentOutOfRange";
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


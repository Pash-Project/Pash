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
        public ErrorRecord ErrorRecord
        {
            get
            {
                return new ErrorRecord(this, "", ErrorCategory.InvalidArgument, null);
            }
        }

        //todo: implement
        //public override string Message { get; }

        public PSArgumentException()
        {
        }

        public PSArgumentException(string message)
            : base(message)
        {
        }

        public PSArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PSArgumentException(string message, string paramName)
            : base(message, paramName)
        {
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


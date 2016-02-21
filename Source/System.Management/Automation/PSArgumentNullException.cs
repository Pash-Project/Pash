// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: implement

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when a parameter's arguement is null.
    /// </summary>
    [Serializable]
    public class PSArgumentNullException : ArgumentNullException, IContainsErrorRecord
    {
        // To be used by the international support.
        internal string id;
        private ErrorRecord error;
        private string message;

        public PSArgumentNullException()
        {
            id = "ArgumentNull";
        }

        public PSArgumentNullException(string paramName)
            : base(paramName)
        {
            id = "ArgumentNull";
        }

        protected PSArgumentNullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            id = "ArgumentNull";
        }

        public PSArgumentNullException(string message, Exception innerException)
            : base(message, innerException)
        {
            id = "ArgumentNull";
        }

        public PSArgumentNullException(string paramName, string message)
            : base(paramName, message)
        {
            id = "ArgumentNull";
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            id = "ArgumentNull";
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

        public override string Message
        {
            get
            {
                if (message == null)
                {
                    message = String.Format("Cannot process argument because the value of argument \"{0}\" is null. Change the value of argument \"{0}\" to a non-null value.", ParamName);
                }
                return message;
            }
        }
    }
}


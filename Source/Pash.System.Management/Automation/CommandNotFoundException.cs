// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Exception is thrown when a command is not found.
    /// </summary>
    [Serializable]
    public class CommandNotFoundException : RuntimeException
    {
        internal string id;
        private ErrorRecord error;

        public string CommandName { get; set; }

        public CommandNotFoundException()
        {
            id = "CommandNotFoundException";
        }

        public CommandNotFoundException(string message)
            : base(message)
        {
            id = "CommandNotFoundException";
        }

        public CommandNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
            id = "CommandNotFoundException";
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            id = "CommandNotFoundException";
        }

        protected CommandNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            id = "CommandNotFoundException";
        }

        // Used by Pash's command manager
        internal CommandNotFoundException(string command, Exception innerException, string errorid)
            : base(null, innerException)
        {
            id = errorid;
            CommandName = command;
        }

        public override ErrorRecord ErrorRecord
        {

            get
            {
                if (error != null)
                {
                    return error;
                }
                error = new ErrorRecord(new ParentContainsErrorRecordException(this), id, ErrorCategory.ObjectNotFound, null);
                return error;
            }
        }
    }
}

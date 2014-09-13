// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    /// <summary>
    /// Exception thrown when there is a serious error running a cmdlet.
    /// </summary>
    [Serializable]
    public class CmdletInvocationException : RuntimeException
    {
        // a little rant..
        // automatic properties don't work the way I wish they would
        // doing something like 
        // public override ErrorRecord ErrorRecord { get; private set; }
        // will return a compiler error! Very annoying.
        // So I am forced to expand the many properties which could be implemented in this way
        // grrrrrrrrrrrrrrr... x 10
        private ErrorRecord errorRecord;
        public override ErrorRecord ErrorRecord
        {
            get
            {
                return errorRecord;
            }
        }

        public CmdletInvocationException()
        {
        }

        public CmdletInvocationException(string message)
            : base(message)
        {
        }

        public CmdletInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CmdletInvocationException(ErrorRecord errorRecord)
            : base("Cmdlet invocation failed")
        {
            this.errorRecord = errorRecord;
        }

        //todo: implement
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        //todo: handle info and context
        protected CmdletInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            errorRecord = null;
        }
    }
}


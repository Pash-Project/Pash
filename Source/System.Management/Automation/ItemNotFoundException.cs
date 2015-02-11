// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class ItemNotFoundException : SessionStateException
    {
        public ItemNotFoundException() : this("Item not found")
        {
        }

        public ItemNotFoundException(string message)
            : this(message, null)
        {
        }

        protected ItemNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ItemNotFoundException(string message, Exception innerException)
            : this(message, innerException, "ItemNotFound")
        {
        }

        internal ItemNotFoundException(string message, Exception innerException, string errorId)
            : base(message, innerException)
        {
            ErrorRecord = new ErrorRecord(this, errorId, ErrorCategory.ObjectNotFound, null);
        }

        internal ItemNotFoundException(string itemName, SessionStateCategory sessionStateCategory, 
                                       string errorIdAndResourceId, params object[] messageArgs)
            : base(itemName, sessionStateCategory, errorIdAndResourceId, ErrorCategory.ObjectNotFound, messageArgs)
        {
        }
    }
}

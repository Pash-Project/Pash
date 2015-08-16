// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class SetValueException : ExtendedTypeSystemException
    {
        public SetValueException()
            : base(typeof(SetValueException).FullName)
        {
        }

        public SetValueException(string message)
            : base(message)
        {
        }

        protected SetValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public SetValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal SetValueException(string message, string errorId, Exception innerException)
            : base(message, errorId, innerException)
        {
        }
    }
}

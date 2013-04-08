// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class SetValueInvocationException : SetValueException
    {
        public SetValueInvocationException()
            : base(typeof(SetValueInvocationException).FullName)
        {
        }

        public SetValueInvocationException(string message)
            : base(message)
        {
        }

        protected SetValueInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public SetValueInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


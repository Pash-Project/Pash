// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class ItemNotFoundException : SessionStateException
    {
        public ItemNotFoundException()
        {
        }

        public ItemNotFoundException(string message)
            : base(message)
        {
        }

        protected ItemNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ItemNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

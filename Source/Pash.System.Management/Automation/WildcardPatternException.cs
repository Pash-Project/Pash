// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class WildcardPatternException : RuntimeException
    {
        public WildcardPatternException(string message)
            : base(message)
        {

        }

        protected WildcardPatternException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public WildcardPatternException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}


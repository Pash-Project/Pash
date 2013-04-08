// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when there is a parser error related to an incomplete statement.
    /// </summary>
    [Serializable]
    public class IncompleteParseException : ParseException
    {
        public IncompleteParseException()
        {
            base.Id = "IncompleteParse";
        }

        public IncompleteParseException(string message)
            : base(message)
        {
            base.Id = "IncompleteParse";
        }

        public IncompleteParseException(string message, Exception innerException)
            : base(message, innerException)
        {
            base.Id = "IncompleteParse";
        }

        protected IncompleteParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


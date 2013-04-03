// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when the script parser has an error.
    /// </summary>
    [Serializable]
    public class ParseException : RuntimeException
    {
        public ParseException()
        {
            base.Id = "Parse";
            base.Category = ErrorCategory.ParserError;
        }

        public ParseException(string message) : base(message)
        {
            base.Id = "Parse";
            base.Category = ErrorCategory.ParserError;
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
            base.Id = "Parse";
            base.Category = ErrorCategory.ParserError;
        }

        protected ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}


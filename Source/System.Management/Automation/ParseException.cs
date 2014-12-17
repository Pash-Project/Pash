// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;
using System.Text;

namespace System.Management.Automation
{
    /// <summary>
    /// Thrown when the script parser has an error.
    /// </summary>
    [Serializable]
    public class ParseException : RuntimeException
    {
        public string RawMessage { get; private set; }
        public string FormattedMessage { get; private set; }

        public override string Message
        {
            get
            {
                return FormattedMessage;
            }
        }

        public ParseException() : this("Unknown Error")
        {
        }

        public ParseException(string rawMessage) : this(rawMessage, -1, -1)
        {
        }

        public ParseException(string rawMessage, int line, int col)
        {
            Id = "Parse";
            Category = ErrorCategory.ParserError;
            RawMessage = rawMessage;
            FormattedMessage = FormatMessage(rawMessage, line, col);
        }

        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        {
            base.Id = "Parse";
            base.Category = ErrorCategory.ParserError;
        }

        protected ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private string FormatMessage(string rawMessage, int line, int col)
        {
            var msg = new StringBuilder("Parse error");
            if (line >= 0 || col >= 0)
            {
                msg.AppendFormat(" at ({0}:{1})", line, col);
            }
            msg.Append(": ");
            if (rawMessage.Length > 100)
            {
                msg.Append(rawMessage.Substring(0, 97));
                msg.Append("...");
            }
            else
            {
                msg.Append(rawMessage);
            }
            return msg.ToString();
        }
    }
}


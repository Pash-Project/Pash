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
        private const int _maxLineLength = 70;
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

        public ParseException(string rawMessage) : this(rawMessage, -1, -1, "")
        {
        }

        public ParseException(string rawMessage, int line, int col, string srcText)
        {
            Id = "Parse";
            Category = ErrorCategory.ParserError;
            RawMessage = rawMessage;
            FormattedMessage = FormatMessage(rawMessage, line, col, srcText);
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

        private string FormatMessage(string rawMessage, int line, int col, string srcText)
        {
            var msg = new StringBuilder("Parse error");
            if (line >= 0 || col >= 0)
            {
                msg.AppendFormat(" at ({0}:{1})", line, col);
            }
            msg.Append(": ");
            if (rawMessage.Length > _maxLineLength)
            {
                msg.Append(rawMessage.Substring(0, _maxLineLength - 3));
                msg.Append("...");
            }
            else
            {
                msg.Append(rawMessage);
            }
            AddErrorLocationDescription(line, col, srcText, msg);
            return msg.ToString();
        }

        private void AddErrorLocationDescription(int line, int col, string srcText, StringBuilder msgBuilder)
        {
            const int leftFromError = 50;
            if (line < 0 || col < 0)
            {
                return;
            }
            var srcLines = srcText.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None);
            if (line > srcLines.Length)
            {
                return;
            }
            var affectedLine = srcLines[line];
            msgBuilder.AppendLine();
            var cutLen = col - leftFromError;
            if (cutLen > 0)
            {
                affectedLine = "..." + affectedLine.Substring(cutLen + 3);
                col -= leftFromError;
            }
            if (affectedLine.Length > _maxLineLength)
            {
                affectedLine = affectedLine.Substring(0, _maxLineLength - 3) + "...";
            }
            msgBuilder.Append("> ");
            msgBuilder.AppendLine(affectedLine);
            msgBuilder.Append("^".PadLeft(col + 3)); // +1 because index offset, + 2 because '> ' in line above
        }
    }
}


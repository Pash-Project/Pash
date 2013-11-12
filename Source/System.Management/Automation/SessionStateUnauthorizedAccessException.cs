using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    public class SessionStateUnauthorizedAccessException : SessionStateException
    {
        public SessionStateUnauthorizedAccessException()
            : base() { }

        public SessionStateUnauthorizedAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public SessionStateUnauthorizedAccessException(string message)
            : base(message) { }

        public SessionStateUnauthorizedAccessException(string message, Exception innerException)
            : base() { }
    }
}


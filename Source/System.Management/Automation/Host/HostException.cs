using System;

namespace System.Management.Automation
{
    public class HostException : RuntimeException
    {
        public HostException() : base()
        {
        }

        public HostException(string message) : base(message)
        {
        }

        public HostException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal HostException(string message, string errorId, ErrorCategory errorCategory, object target)
            : base(message, errorId, errorCategory, target)
        {
        }
    }
}


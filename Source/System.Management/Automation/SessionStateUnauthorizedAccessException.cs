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

        internal SessionStateUnauthorizedAccessException(string itemName, SessionStateCategory sessionStateCategory, 
                                                         string errorIdAndResourceId, params object[] messageArgs)
            : base(itemName, sessionStateCategory, errorIdAndResourceId, ErrorCategory.WriteError, messageArgs)
        {
        }

        internal SessionStateUnauthorizedAccessException(
            string message,
            string itemName,
            SessionStateCategory sessionStateCategory)
            : base(message, itemName, sessionStateCategory)
        {
        }

        internal static SessionStateUnauthorizedAccessException CreateVariableNotWritableError(PSVariable variable)
        {
            var ex = new SessionStateUnauthorizedAccessException(
                String.Format("Cannot overwrite variable {0} because it is read-only or constant.", variable.Name),
                variable.Name,
                SessionStateCategory.Variable);

            var error = new ErrorRecord(
                new ParentContainsErrorRecordException(ex),
                "VariableNotWritable",
                ErrorCategory.WriteError,
                variable.Name);
            error.CategoryInfo.TargetName = variable.Name;
            error.CategoryInfo.TargetType = "String";

            ex.ErrorRecord = error;
            ex.Source = typeof(PSVariable).Namespace;

            return ex;
        }
    }
}


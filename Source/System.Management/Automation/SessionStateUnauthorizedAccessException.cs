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
            string message = String.Format("Cannot overwrite variable {0} because it is read-only or constant.", variable.Name);
            return CreateError(variable, message, "VariableNotWritable");
        }

        static SessionStateUnauthorizedAccessException CreateError(
            PSVariable variable,
            string message,
            string errorId)
        {
            var ex = new SessionStateUnauthorizedAccessException(
                message,
                variable.Name,
                SessionStateCategory.Variable);

            var error = new ErrorRecord(
                new ParentContainsErrorRecordException(ex),
                errorId,
                ErrorCategory.WriteError,
                variable.Name);

            ex.ErrorRecord = error;
            ex.Source = typeof(PSVariable).Namespace;

            return ex;
        }

        internal static Exception CreateVariableCannotBeMadeConstantError(PSVariable variable)
        {
            string message = String.Format("Existing variable {0} cannot be made constant. Variables can be made constant only at creation time.", variable.Name);
            return CreateError(variable, message, "VariableCannotBeMadeConstant");
        }
    }
}


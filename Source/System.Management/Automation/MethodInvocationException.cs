// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    /// <summary>
    /// Called when Pash has a problem invoking a method.
    /// </summary>
    [Serializable]
    public class MethodInvocationException : MethodException
    {
        public override ErrorRecord ErrorRecord { get; set; }

        public MethodInvocationException()
            : this(typeof(MethodInvocationException).FullName)
        {
        }

        public MethodInvocationException(string message)
            : this(message, null)
        {
        }

        public MethodInvocationException(string message, Exception innerException)
            : this(message, innerException, null, ErrorCategory.NotSpecified)
        {
        }

        public MethodInvocationException(string message, Exception innerException, string errorId, ErrorCategory errorCategory)
            : base(message, innerException)
        {
            var runtimeException = innerException as IContainsErrorRecord;
            if (errorId == null)
            {
                errorId = runtimeException == null ? "MethodInvocation" : runtimeException.ErrorRecord.ErrorId;
            }
            if (errorCategory == ErrorCategory.NotSpecified && runtimeException != null)
            {
                errorCategory = runtimeException.ErrorRecord.CategoryInfo.Category;
            }
            ErrorRecord = new ErrorRecord(this, errorId, errorCategory, null);
        }

        protected MethodInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


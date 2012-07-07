using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace System.Management.Automation
{
    [Serializable]
    public class ErrorRecord : ISerializable
    {
        protected ErrorRecord(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public ErrorRecord(Exception exception, string errorId, ErrorCategory errorCategory, object targetObject)
        {
            Exception = exception;
            FullyQualifiedErrorId = errorId;
            TargetObject = targetObject;
        }

        internal ErrorRecord(ErrorRecord errorRecord, Exception exception)
        {
            Exception = exception;
            FullyQualifiedErrorId = errorRecord.FullyQualifiedErrorId;
            TargetObject = errorRecord.TargetObject;
        }


        // public ErrorCategoryInfo CategoryInfo { get; }
        // public ErrorDetails ErrorDetails { get; set; }
        public Exception Exception { get; internal set; }
        public string FullyQualifiedErrorId { get; internal set; }
        // public InvocationInfo InvocationInfo { get; }
        public object TargetObject { get; internal set; }

        public override string ToString()
        {
            // TODO: implement ErrorRecord.ToString
            return Exception.ToString();
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

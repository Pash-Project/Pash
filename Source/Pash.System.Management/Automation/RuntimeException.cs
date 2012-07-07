using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class RuntimeException : SystemException, IContainsErrorRecord
    {
        public RuntimeException() { throw new NotImplementedException(); }
        public RuntimeException(string message) { throw new NotImplementedException(); }
        protected RuntimeException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }
        public RuntimeException(string message, Exception innerException) { throw new NotImplementedException(); }

        public virtual ErrorRecord ErrorRecord { get; private set; }
        public override string StackTrace { get { throw new NotImplementedException(); } }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        // internals
        //internal static void LockStackTrace(System.Exception e);
        //internal static System.Exception RetrieveException(System.Management.Automation.ErrorRecord errorRecord);
        //internal static string RetrieveMessage(System.Exception e);
        //internal static string RetrieveMessage(System.Management.Automation.ErrorRecord errorRecord);
        //internal RuntimeException(string message, System.Exception innerException, System.Management.Automation.ErrorRecord errorRecord);
        //internal void SetErrorCategory(System.Management.Automation.ErrorCategory errorCategory);
        //internal void SetErrorId(string errorId);
        //internal void SetTargetObject(object targetObject);
        //internal bool SuppressPromptInInterpreter { set; get; }
        //internal bool WasThrownFromThrowStatement { set; get; }
    }
}

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    [Serializable]
    public class SessionStateException : RuntimeException
    {
        public SessionStateException() { throw new NotImplementedException(); }
        public SessionStateException(string message) { throw new NotImplementedException(); }
        protected SessionStateException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }
        public SessionStateException(string message, Exception innerException) { throw new NotImplementedException(); }
        internal SessionStateException(string itemName, SessionStateCategory sessionStateCategory, string errorIdAndResourceId, ErrorCategory errorCategory, params object[] messageArgs)
            { throw new NotImplementedException(); }
        private static string BuildMessage(string itemName, string resourceId, params object[] messageArgs)
            { throw new NotImplementedException(); }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public override ErrorRecord ErrorRecord { get { throw new NotImplementedException(); } }
        public string ItemName { get { throw new NotImplementedException(); } }
        public SessionStateCategory SessionStateCategory { get { throw new NotImplementedException(); } }
    }

 

}
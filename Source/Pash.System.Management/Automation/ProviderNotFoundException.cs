using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class ProviderNotFoundException : SessionStateException
    {
        public ProviderNotFoundException() { throw new NotImplementedException(); }
        public ProviderNotFoundException(string message) { throw new NotImplementedException(); }
        protected ProviderNotFoundException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }
        public ProviderNotFoundException(string message, Exception innerException) { throw new NotImplementedException(); }
        internal ProviderNotFoundException(string itemName, SessionStateCategory sessionStateCategory, string errorIdAndResourceId, params object[] messageArgs)
            { throw new NotImplementedException(); }
    }
}
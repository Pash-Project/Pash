using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class CommandNotFoundException : RuntimeException
    {
        public CommandNotFoundException() { throw new NotImplementedException(); }
        public CommandNotFoundException(string message) { throw new NotImplementedException(); }
        protected CommandNotFoundException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }
        public CommandNotFoundException(string message, Exception innerException) { throw new NotImplementedException(); }

        public string CommandName { get; set; }
        public override ErrorRecord ErrorRecord { get { throw new NotImplementedException(); } }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }

        // internals
        internal CommandNotFoundException(string commandName, Exception innerException, string errorIdAndResourceId, params object[] messageArgs) { throw new NotImplementedException(); }
    }
}

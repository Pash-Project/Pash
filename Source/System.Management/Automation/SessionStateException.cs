// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
    [Serializable]
    public class SessionStateException : RuntimeException
    {
        public string ItemName { get; private set; }
        public SessionStateCategory SessionStateCategory { get; private set; }
        public override ErrorRecord ErrorRecord { get; set; }

        public SessionStateException() : base()
        {
        }

        public SessionStateException(string message) : base(message)
        {
        }

        protected SessionStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SessionStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SessionStateException(string itemName, SessionStateCategory sessionStateCategory, 
                                       string errorIdAndResourceId, ErrorCategory errorCategory,
                                       params object[] messageArgs)
            : base(String.Format("The {0} \"{1}\" ({2}) caused the following error: {3}",
                                 new object[] {sessionStateCategory.ToString(), itemName, errorIdAndResourceId, 
                                               errorCategory.ToString()}))
        {
            //TODO: make this better
            SessionStateCategory = sessionStateCategory;
            ErrorRecord = new ErrorRecord(this, errorIdAndResourceId, errorCategory, null);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

    }

}

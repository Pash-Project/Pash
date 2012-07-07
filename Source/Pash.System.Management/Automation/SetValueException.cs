using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class SetValueException : ExtendedTypeSystemException
    {
        internal const string ReadOnlyProperty = "ReadOnlyProperty";
        internal const string SetWithoutSetterExceptionMsg = "SetWithoutSetterException";
        internal const string XmlNodeSetRestrictions = "XmlNodeSetShouldBeAString";
        internal const string XmlNodeSetShouldBeAString = "XmlNodeSetShouldBeAString";

        public SetValueException() { throw new NotImplementedException(); }
        public SetValueException(string message) { throw new NotImplementedException(); }
        protected SetValueException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }
        public SetValueException(string message, Exception innerException) { throw new NotImplementedException(); }

        // internal SetValueException(string errorId, Exception innerException, string baseName, string resourceId, params object[] arguments);
    }

 

}
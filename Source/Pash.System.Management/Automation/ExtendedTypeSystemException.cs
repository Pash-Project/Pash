using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable]
    public class ExtendedTypeSystemException : RuntimeException
    {
        internal const string AccessMemberOutsidePSObjectMsg = "AccessMemberOutsidePSObject";
        internal const string BaseName = "ExtendedTypeSystem";
        internal const string CannotAddPropertyOrMethodMsg = "CannotAddPropertyOrMethod";
        internal const string CannotChangeReservedMemberMsg = "CannotChangeReservedMember";
        internal const string CannotSetValueForMemberTypeMsg = "CannotSetValueForMemberType";
        internal const string ChangeStaticMemberMsg = "ChangeStaticMember";
        internal const string CodeMethodMethodFormatMsg = "CodeMethodMethodFormat";
        internal const string CodePropertyGetterAndSetterNullMsg = "CodePropertyGetterAndSetterNull";
        internal const string CodePropertyGetterFormatMsg = "CodePropertyGetterFormat";
        internal const string CodePropertySetterFormatMsg = "CodePropertySetterFormat";
        internal const string CycleInAliasMsg = "CycleInAlias";
        internal const string EnumerationExceptionMsg = "EnumerationException";
        internal const string ExceptionGettingMemberMsg = "ExceptionGettingMember";
        internal const string ExceptionGettingMembersMsg = "ExceptionGettingMembers";
        internal const string ExceptionRetrievingMethodDefinitionsMsg = "ExceptionRetrievingMethodDefinitions";
        internal const string ExceptionRetrievingMethodStringMsg = "ExceptionRetrievingMethodString";
        internal const string ExceptionRetrievingParameterizedPropertyDefinitionsMsg = "ExceptionRetrievingParameterizedPropertyDefinitions";
        internal const string ExceptionRetrievingParameterizedPropertyReadStateMsg = "ExceptionRetrievingParameterizedPropertyReadState";
        internal const string ExceptionRetrievingParameterizedPropertyStringMsg = "ExceptionRetrievingParameterizedPropertyString";
        internal const string ExceptionRetrievingParameterizedPropertytypeMsg = "ExceptionRetrievingParameterizedPropertytype";
        internal const string ExceptionRetrievingParameterizedPropertyWriteStateMsg = "ExceptionRetrievingParameterizedPropertyWriteState";
        internal const string ExceptionRetrievingPropertyAttributesMsg = "ExceptionRetrievingPropertyAttributes";
        internal const string ExceptionRetrievingPropertyReadStateMsg = "ExceptionRetrievingPropertyReadState";
        internal const string ExceptionRetrievingPropertyStringMsg = "ExceptionRetrievingPropertyString";
        internal const string ExceptionRetrievingPropertyTypeMsg = "ExceptionRetrievingPropertyType";
        internal const string ExceptionRetrievingPropertyWriteStateMsg = "ExceptionRetrievingPropertyWriteState";
        internal const string ExceptionRetrievingTypeNameHierarchyMsg = "ExceptionRetrievingTypeNameHierarchy";
        internal const string MemberAlreadyPresentFromTypesXmlMsg = "MemberAlreadyPresentFromTypesXml";
        internal const string MemberAlreadyPresentMsg = "MemberAlreadyPresent";
        internal const string MemberNotPresentMsg = "MemberNotPresent";
        internal const string NotAClsCompliantFieldPropertyMsg = "NotAClsCompliantFieldProperty";
        internal const string NotTheSameTypeOrNotIcomparableMsg = "NotTheSameTypeOrNotIcomparable";
        internal const string PropertyNotFoundInTypeDescriptorMsg = "PropertyNotFoundInTypeDescriptor";
        internal const string ReservedMemberNameMsg = "ReservedMemberName";
        internal const string ToStringExceptionMsg = "ToStringException";
        internal const string TypesXmlErrorMsg = "TypesXmlError";

        public ExtendedTypeSystemException() { throw new NotImplementedException(); }
        public ExtendedTypeSystemException(string message) { throw new NotImplementedException(); }
        protected ExtendedTypeSystemException(SerializationInfo info, StreamingContext context) { throw new NotImplementedException(); }
        public ExtendedTypeSystemException(string message, Exception innerException) { throw new NotImplementedException(); }
        internal ExtendedTypeSystemException(string errorId, Exception innerException, string baseName, string resourceId, params object[] arguments)
        { throw new NotImplementedException(); }
    }

 

}
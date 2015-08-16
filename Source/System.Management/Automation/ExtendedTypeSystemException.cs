// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
        internal const string CannotChangePSMethodInfoValue = "CannotChangePSMethodInfoValue";
        internal const string CatchFromBaseParameterizedPropertyAdapterGetValue = "CatchFromBaseParameterizedPropertyAdapterGetValue";
        internal const string CatchFromBaseAdapterParameterizedPropertySetValue = "CatchFromBaseAdapterParameterizedPropertySetValue";
        internal const string MethodCountCouldNotFindBest = "MethodCountCouldNotFindBest";

        public ExtendedTypeSystemException()
            : base()
        {
        }

        public ExtendedTypeSystemException(string message)
            : base(message)
        {
        }

        protected ExtendedTypeSystemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        public ExtendedTypeSystemException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        internal ExtendedTypeSystemException(string errorId, Exception innerException,
                                             string baseName, string resourceId, params object[] arguments)
        {
            throw new NotImplementedException(); 
        }

        internal ExtendedTypeSystemException(string message, string errorId, Exception innerException)
            : base(message, innerException)
        {
            CreateErrorRecord(errorId);
        }

        internal ExtendedTypeSystemException(string message, string errorId)
            : base(message)
        {
            CreateErrorRecord(errorId);
        }

        private void CreateErrorRecord(string errorId)
        {
            ErrorRecord = new ErrorRecord(new ParentContainsErrorRecordException(this),
                errorId,
                ErrorCategory.NotSpecified,
                null);
        }
    }

}

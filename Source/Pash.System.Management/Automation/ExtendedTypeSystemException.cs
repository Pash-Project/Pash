// Copyright (C) Pash Contributors. All Rights Reserved. See https://github.com/Pash-Project/Pash/

#region BSD License
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, (either expressed or implied, of the FreeBSD Project.
#endregion

#region GPL License
// This file is part of Pash.
//
// Pash is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Pash is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along
// with Pash.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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

// Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

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

using System;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    [TypeDescriptionProvider(typeof(PSObjectTypeDescriptionProvider))]
    public class PSObject : IFormattable, IComparable
    {
        public const string AdaptedMemberSetName = "PSAdapted";
        public const string BaseObjectMemberSetName = "PSBase";
        public const string ExtendedMemberSetName = "PSExtended";

        public object BaseObject
        {
            get
            {
                object objParent = null;
                PSObject obj = this;
                do
                {
                    objParent = obj.ImmediateBaseObject;
                    obj = objParent as PSObject;
                }
                while (obj != null);
                return objParent;
            }
        }

        public object ImmediateBaseObject { get; private set; }
        public PSMemberInfoCollection<PSMemberInfo> Members { get; private set; }
        public PSMemberInfoCollection<PSMethodInfo> Methods { get; private set; }
        public PSMemberInfoCollection<PSPropertyInfo> Properties { get; private set; }
        public Collection<string> TypeNames { get; private set; }

        public PSObject()
        {
            // TODO: decide what to do in the default case
            Initialize(PSCustomObject.Instance);
        }

        public PSObject(object obj)
        {
            Initialize(obj);
        }

        public static PSObject AsPSObject(object obj)
        {
            // TODO: should the object be copied or self returned?
            if (obj is PSObject)
                return (PSObject)obj;

            return new PSObject(obj);
        }

        protected void Initialize(object obj)
        {
            Members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            Properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
            Methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            ImmediateBaseObject = obj;
        }

        public virtual PSObject Copy() { throw new NotImplementedException(); }

        public override bool Equals(object obj)
        {
            return ImmediateBaseObject.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ImmediateBaseObject.GetHashCode();
        }

        public override string ToString()
        {
            return ImmediateBaseObject.ToString();
        }

        // internal
        //internal static object Base(object obj);
        //internal PSMemberInfoInternalCollection<PSPropertyInfo> GetAdaptedProperties();
        //internal PSMemberInfoInternalCollection<PSPropertyInfo> GetBaseProperties();
        //internal static object GetNoteSettingValue(.PSMemberSet settings, string noteName, object defaultValue, System.Type expectedType);
        //internal int GetReferenceHashCode();
        //internal PSMemberInfoInternalCollection<PSPropertyInfo> GetSpecificPropertiesToSerialize();
        //internal static PSMemberInfo GetStaticCLRMember(object obj, string methodName);
        //internal TypeTable GetTypeTable();
        //internal void SetCoreOnDeserialization(object value, bool overrideTypeInfo);
        //internal bool ShouldSerializeAdapter();
        //internal bool ShouldSerializeBase();
        //internal static string ToString(ExecutionContext context, object obj, string separator, string format, IFormatProvider formatProvider, bool recurse, bool unravelEnumeratorOnRecurse);
        //internal static string ToStringEnumerable(ExecutionContext context, IEnumerable enumerable, string separator, string format, IFormatProvider formatProvider);
        //internal static string ToStringEnumerator(ExecutionContext context, IEnumerator enumerator, string separator, string format, IFormatProvider formatProvider);
        //internal static string ToStringParser(ExecutionContext context, object obj);
        //internal static PSMemberInfoInternalCollection<U> TransformMemberInfoCollection<T, U>(PSMemberInfoCollection<T> source)
        //    where T : PSMemberInfo
        //    where U : PSMemberInfo;
        //internal static T TypeTableGetMemberDelegate<T>(PSObject msjObj, string name)
        //    where T : PSMemberInfo;
        //internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(PSObject msjObj)
        //    where T : PSMemberInfo;
        //internal PSMemberInfoInternalCollection<PSPropertyInfo> adaptedMembers;
        //internal Adapter adapter;
        //internal Adapter clrAdapter;
        //internal PSMemberInfoInternalCollection<PSPropertyInfo> clrMembers;
        //internal ExecutionContext context;
        //internal static readonly DotNetAdapter dotNetInstanceAdapter;
        //internal bool hasGeneratedReservedMembers;
        //internal bool immediateBaseObjectIsEmpty;
        //internal PSMemberInfoInternalCollection<PSMemberInfo> instanceMembers;
        //internal bool isDeserialized;
        //internal static PSTraceSource memberResolution;
        //internal bool preserveToString;
        //internal bool preserveToStringSet;
        //internal string TokenText;
        //internal ConsolidatedString typeNames;
        //internal const string MshTypeNames;
        //internal const string PSObjectMemberSetName;

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

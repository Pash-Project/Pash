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

using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class PSMemberSet : PSMemberInfo
    {
        public bool InheritMembers { get; private set; }
        public PSMemberInfoCollection<PSMemberInfo> Members { get; private set; }
        public override PSMemberTypes MemberType { get { return PSMemberTypes.MemberSet; } }
        public PSMemberInfoCollection<PSMethodInfo> Methods { get; private set; }
        public PSMemberInfoCollection<PSPropertyInfo> Properties { get; private set; }

        public override object Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new InvalidOperationException("Can't change this value.");
            }
        }

        internal bool inheritMembers;
        internal PSMemberInfoCollectionImplementation<PSMemberInfo> internalMembers;

        public PSMemberSet(string name)
        {
            Name = name;
            InheritMembers = true;
            Members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            Methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            Properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
        }

        public PSMemberSet(string name, IEnumerable<PSMemberInfo> members)
            : this(name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Name can't be empty");

            if (members == null)
                throw new NullReferenceException("Members collection can't be null");
        }

        internal PSMemberSet(string name, PSObject obj)
            : this(name)
        {

        }

        public override PSMemberInfo Copy()
        {
            PSMemberSet outVal = new PSMemberSet(Name);
            foreach (PSMemberInfo member in Members)
            {
                outVal.Members.Add(member);
            }
            CopyProperties(outVal);
            return outVal;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(" {");

            foreach (PSMemberInfo info in Members)
            {
                builder.Append(info.Name);
                builder.Append(", ");
            }

            // remove the last coma
            if (builder.Length > 2)
            {
                builder.Remove(builder.Length - 2, 2);
            }

            builder.Insert(0, Name);
            builder.Append("}");

            return builder.ToString();

        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSMemberSet).FullName;
            }
        }
    }
}

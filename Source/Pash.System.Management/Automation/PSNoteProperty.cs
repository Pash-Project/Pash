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

using System.Text;

namespace System.Management.Automation
{
    public class PSNoteProperty : PSPropertyInfo
    {
        static PSNoteProperty()
        {

        }

        public PSNoteProperty(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Name can't be empty");

            Name = name;
            _value = value;
        }

        private object _value;
        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!IsInstance)
                    // TODO: throw the SetValueException instead
                    throw new Exception("Can't change value of a static note.");

                _value = value;
            }
        }

        public override bool IsGettable
        {
            get
            {
                return true;
            }
        }

        public override bool IsSettable
        {
            get
            {
                return IsInstance;
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.NoteProperty;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                if (Value == null)
                    return string.Empty;

                return Value.GetType().FullName;
            }
        }

        public override PSMemberInfo Copy()
        {
            PSNoteProperty outVal = new PSNoteProperty(Name, Value);
            CopyProperties(outVal);
            return outVal;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(TypeNameOfValue);
            str.Append(" ");
            str.Append(Name);
            str.Append("=");
            str.Append((Value == null) ? "null" : Value.ToString());

            return str.ToString();
        }
    }
}

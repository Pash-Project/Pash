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
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
    public class PSVariable
    {
        public string Name { get; private set; }
        public virtual string Description { get; set; }
        public virtual object Value { get; set; }
        public virtual ScopedItemOptions Options { get; set; }
        public Collection<Attribute> Attributes { get; private set; }

        public PSVariable(string name)
            : this(name, null, ScopedItemOptions.None, null)
        {
        }

        public PSVariable(string name, object value)
            : this(name, value, ScopedItemOptions.None, null)
        {
        }

        public PSVariable(string name, object value, ScopedItemOptions options)
            : this(name, value, options, null)
        {
        }

        public PSVariable(string name, object value, ScopedItemOptions options, Collection<Attribute> attributes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new NullReferenceException("Variable name can't be empty");
            }

            Name = name;
            Description = string.Empty;
            Value = value;
            Options = options;

            // Copy attributes
            Attributes = new Collection<Attribute>();
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    Attributes.Add(attribute);
                }
            }
        }

        public virtual bool IsValidValue(object value)
        {
            throw new NotImplementedException();
        }

        // internals
        //internal static bool IsValidValue(object value, System.Attribute attribute);
        //internal PSVariable(string name, object value, System.Management.Automation.ScopedItemOptions options, System.Collections.ObjectModel.Collection<Attribute> attributes, string description);
        //internal PSVariable(string name, object value, System.Management.Automation.ScopedItemOptions options, string description);
        //internal void SetOptions(System.Management.Automation.ScopedItemOptions newOptions, bool force);
        //internal void SetValueRaw(object newValue, bool preserveValueTypeSemantics);
        //internal object TransformValue(object value);
        //internal bool IsAllScope { get; }
        //internal bool IsConstant { get; }
        internal bool IsPrivate { get; private set; }
        //internal bool IsReadOnly { get; }
        //internal bool WasRemoved { set; get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('$');
            sb.Append(Name);
            sb.Append(" = ");
            sb.Append(Value.ToString());

            return sb.ToString();
        }
    }
}

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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("Variable", ProviderCapabilities.ShouldProcess)]
    public sealed class VariableProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Variable";

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo> { new PSDriveInfo("Variable", base.ProviderInfo) };
        }

        internal override object GetSessionStateItem(string name)
        {
            // TODO: deal with empty path
            if (string.Equals("variable:\\", name, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return SessionState.SessionStateGlobal.GetVariable(name);
        }

        internal override bool CanRenameItem(object item)
        {
            PSVariable variable = item as PSVariable;

            if (variable == null)
                return false;

            // TODO: the rename can be Force'ed
            if (((variable.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) ||
                ((variable.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None))
                return false;

            return true;
        }

        internal override IDictionary GetSessionStateTable()
        {
            return (IDictionary)SessionState.SessionStateGlobal.GetVariables();
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            PSVariable variable = null;
            if (value != null)
            {
                variable = value as PSVariable;
                if (variable == null)
                {
                    variable = new PSVariable(name, value);
                }
                else if (String.Compare(name, variable.Name, true, System.Globalization.CultureInfo.CurrentCulture) != 0)
                {
                    PSVariable var = new PSVariable(name, variable.Value, variable.Options, variable.Attributes);
                    var.Description = variable.Description;
                    variable = var;
                }
            }
            else
            {
                variable = new PSVariable(name, null);
            }
            // TODO: can be Force'ed
            PSVariable item = base.SessionState.SessionStateGlobal.SetVariable(variable) as PSVariable;
            if (writeItem && (item != null))
            {
                WriteItemObject(item, item.Name, false);
            }
        }

        internal override void RemoveSessionStateItem(string name)
        {
            // TODO: can be Force'ed
            SessionState.SessionStateGlobal.RemoveVariable(name);
        }

        protected override void GetItem(string name)
        {
            // HACK: should it be this way?

            if (string.Equals("variable:\\", name, StringComparison.CurrentCultureIgnoreCase))
            {
                name = PathIntrinsics.RemoveDriveName(name);
                GetChildItems(name, false);
            }
            else
            {
                GetItem(PathIntrinsics.RemoveDriveName(name));
            }
        }
    }
}

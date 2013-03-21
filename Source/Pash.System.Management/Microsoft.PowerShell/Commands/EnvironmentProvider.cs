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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("Environment", ProviderCapabilities.ShouldProcess)]
    public sealed class EnvironmentProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Environment";

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo> { new PSDriveInfo("Env", base.ProviderInfo) };
        }

        internal override object GetSessionStateItem(string name)
        {
            string environmentVariable = Environment.GetEnvironmentVariable(name);
            if (environmentVariable != null)
            {
                return new DictionaryEntry(name, environmentVariable);
            }
            return null;
        }

        internal override IDictionary GetSessionStateTable()
        {
            var dictionary = new Dictionary<string, DictionaryEntry>(StringComparer.CurrentCultureIgnoreCase);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                dictionary.Add((string)entry.Key, entry);
            }
            return dictionary;
        }

        internal override void RemoveSessionStateItem(string name)
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            if (value == null)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
            else
            {
                if (value is DictionaryEntry)
                {
                    value = ((DictionaryEntry)value).Value;
                }
                string str = value as string;
                if (str == null)
                {
                    str = PSObject.AsPSObject(value).ToString();
                }
                Environment.SetEnvironmentVariable(name, str);
                DictionaryEntry item = new DictionaryEntry(name, str);
                if (writeItem)
                {
                    WriteItemObject(item, name, false);
                }
            }
        }
    }
}

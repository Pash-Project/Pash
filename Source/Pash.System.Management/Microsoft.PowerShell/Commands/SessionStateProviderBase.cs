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
using System.Text;
using System.Management.Automation.Provider;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
    {
        protected SessionStateProviderBase()
        {
        }

        protected override void ClearItem(string path) { throw new NotImplementedException(); }
        protected override void CopyItem(string path, string copyPath, bool recurse) { throw new NotImplementedException(); }

        protected override void GetChildItems(string path, bool recurse)
        {
            if (path == "\\")
                path = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                IDictionary sessionStateTable = GetSessionStateTable();

                foreach (DictionaryEntry entry in sessionStateTable)
                {
                    WriteItemObject(entry.Value, (string)entry.Key, false);
                }
            }
            else
            {
                object item = GetSessionStateItem(path);

                if (item != null)
                {
                    WriteItemObject(item, path, false);
                }
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers) { throw new NotImplementedException(); }
        protected override void GetItem(string name) { throw new NotImplementedException(); }
        protected override bool HasChildItems(string path) { throw new NotImplementedException(); }
        protected override bool IsValidPath(string path) { throw new NotImplementedException(); }

        protected override bool ItemExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            return null != GetSessionStateItem(path);
        }

        protected override void NewItem(string path, string type, object newItem) { throw new NotImplementedException(); }
        protected override void RemoveItem(string path, bool recurse) { throw new NotImplementedException(); }
        protected override void RenameItem(string name, string newName) { throw new NotImplementedException(); }
        protected override void SetItem(string name, object value) { throw new NotImplementedException(); }

        // internals
        internal virtual bool CanRenameItem(object item)
        {
            return true;
        }

        internal abstract object GetSessionStateItem(string name);
        internal abstract IDictionary GetSessionStateTable();
        internal virtual object GetValueOfItem(object item)
        {
            if (item is DictionaryEntry)
            {
                return ((DictionaryEntry)item).Value;
            }
            return item;
        }
        internal abstract void RemoveSessionStateItem(string name);
        internal abstract void SetSessionStateItem(string name, object value, bool writeItem);

        #region IContentCmdletProvider Members

        public void ClearContent(string path)
        {
            throw new NotImplementedException();
        }

        public object ClearContentDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        public IContentReader GetContentReader(string path)
        {
            throw new NotImplementedException();
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        public IContentWriter GetContentWriter(string path)
        {
            throw new NotImplementedException();
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

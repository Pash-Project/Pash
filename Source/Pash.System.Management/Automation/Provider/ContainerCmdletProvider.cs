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
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation.Provider
{
    public abstract class ContainerCmdletProvider : ItemCmdletProvider
    {
        protected ContainerCmdletProvider()
        {
        }

        protected virtual void CopyItem(string path, string copyPath, bool recurse) { throw new NotImplementedException(); }
        protected virtual object CopyItemDynamicParameters(string path, string destination, bool recurse) { throw new NotImplementedException(); }
        protected virtual void GetChildItems(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual object GetChildItemsDynamicParameters(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual void GetChildNames(string path, ReturnContainers returnContainers) { throw new NotImplementedException(); }
        protected virtual object GetChildNamesDynamicParameters(string path) { throw new NotImplementedException(); }
        protected virtual bool HasChildItems(string path) { throw new NotImplementedException(); }
        protected virtual void NewItem(string path, string itemTypeName, object newItemValue) { throw new NotImplementedException(); }
        protected virtual object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue) { throw new NotImplementedException(); }
        protected virtual void RemoveItem(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual object RemoveItemDynamicParameters(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual void RenameItem(string path, string newName) { throw new NotImplementedException(); }
        protected virtual object RenameItemDynamicParameters(string path, string newName) { throw new NotImplementedException(); }

        // internals
        //internal void CopyItem(string path, string copyPath, bool recurse, CmdletProviderContext context);
        //internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context);
        //internal void GetChildItems(string path, bool recurse, CmdletProviderContext context);
        //internal object GetChildItemsDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal void GetChildNames(string path, ReturnContainers returnContainers, CmdletProviderContext context);
        //internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context);
        //internal bool HasChildItems(string path, CmdletProviderContext context);
        //internal void NewItem(string path, string type, object newItemValue, CmdletProviderContext context);
        //internal object NewItemDynamicParameters(string path, string type, object newItemValue, CmdletProviderContext context);
        //internal void RemoveItem(string path, bool recurse, CmdletProviderContext context);
        //internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal void RenameItem(string path, string newName, CmdletProviderContext context);
        //internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context);

        internal void GetChildItems(string path, bool recurse, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            GetChildItems(path, recurse);
        }
    }
}

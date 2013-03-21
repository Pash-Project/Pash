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
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    public sealed class ItemCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public Collection<PSObject> Clear(string path) { throw new NotImplementedException(); }
        public Collection<PSObject> Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers) { throw new NotImplementedException(); }
        public bool Exists(string path) { throw new NotImplementedException(); }
        public Collection<PSObject> Get(string path) { throw new NotImplementedException(); }
        public void Invoke(string path) { throw new NotImplementedException(); }
        public bool IsContainer(string path) { throw new NotImplementedException(); }
        public Collection<PSObject> Move(string path, string destination) { throw new NotImplementedException(); }
        public Collection<PSObject> New(string path, string name, string itemTypeName, object content) { throw new NotImplementedException(); }
        public void Remove(string path, bool recurse) { throw new NotImplementedException(); }
        public Collection<PSObject> Rename(string path, string newName) { throw new NotImplementedException(); }
        public Collection<PSObject> Set(string path, object value) { throw new NotImplementedException(); }

        // internals
        //internal void Clear(string path, CmdletProviderContext context);
        //internal object ClearItemDynamicParameters(string path, CmdletProviderContext context);
        //internal void Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers, CmdletProviderContext context);
        //internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context);
        //internal bool Exists(string path, CmdletProviderContext context);
        //internal void Get(string path, CmdletProviderContext context);
        //internal object GetItemDynamicParameters(string path, CmdletProviderContext context);
        //internal void Invoke(string path, CmdletProviderContext context);
        //internal object InvokeItemDynamicParameters(string path, CmdletProviderContext context);
        //internal bool IsContainer(string path, CmdletProviderContext context);
        //internal ItemCmdletProviderIntrinsics(SessionStateInternal sessionState);
        //internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context);
        //internal void Move(string path, string destination, CmdletProviderContext context);
        //internal object MoveItemDynamicParameters(string path, string destination, CmdletProviderContext context);
        //internal void New(string path, string name, string type, object content, CmdletProviderContext context);
        //internal object NewItemDynamicParameters(string path, string type, object content, CmdletProviderContext context);
        //internal void Remove(string path, bool recurse, CmdletProviderContext context);
        //internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal void Rename(string path, string newName, CmdletProviderContext context);
        //internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context);
        //internal void Set(string path, object value, CmdletProviderContext context);
        //internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context);
    }
}

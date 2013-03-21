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
using Pash.Implementation;

namespace System.Management.Automation.Provider
{
    public abstract class NavigationCmdletProvider : ContainerCmdletProvider
    {
        private ProviderRuntime _providerRuntime;

        protected NavigationCmdletProvider()
        {
        }

        protected virtual string GetChildName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            path = PathIntrinsics.NormalizePath(path);
            path = path.TrimEnd(PathIntrinsics.CorrectSlash);

            int iLastSlash = path.LastIndexOf('\\');
            if (iLastSlash == -1)
            {
                return path;
            }

            return path.Substring(iLastSlash + 1);
        }

        internal string GetChildName(string path, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return GetChildName(path);
        }

        protected virtual string GetParentPath(string path, string root)
        {
            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("Path can't be empty");

            if ((root == null) && (PSDriveInfo != null))
            {
                root = PSDriveInfo.Root;
            }

            path = PathIntrinsics.NormalizePath(path);
            path = path.TrimEnd(PathIntrinsics.CorrectSlash);

            if (root != null)
            {
                if (string.Equals(path, root, StringComparison.CurrentCultureIgnoreCase))
                {
                    return string.Empty;
                }
            }

            int iLastSlash = path.LastIndexOf(PathIntrinsics.CorrectSlash);

            if (iLastSlash > 0)
                return path.Substring(0, iLastSlash);

            if (iLastSlash == 1)
                return PathIntrinsics.CorrectSlash.ToString();

            return string.Empty;
        }

        internal string GetParentPath(string path, string root, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return GetParentPath(path, root);
        }

        protected virtual bool IsItemContainer(string path)
        {
            throw new NotImplementedException();
        }

        internal bool IsItemContainer(string path, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return IsItemContainer(path);
        }

        protected virtual string MakePath(string parent, string child)
        {
            if ((parent == null) && (child == null))
            {
                throw new NullReferenceException("Can't call MakePath with null values.");
            }
            if (string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
            {
                return NormalizePath(child);
            }

            parent = NormalizePath(parent);
            if (!string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
            {
                if (parent.EndsWith("\\"))
                {
                    return parent;
                }
                else
                {
                    return parent + '\\';
                }
            }

            child = NormalizePath(child);
            StringBuilder builder = new StringBuilder(parent);

            if (!parent.EndsWith("\\"))
                builder.Append("\\");

            // Make sure we do not add two \
            if (child.StartsWith("\\"))
            {
                builder.Append(child, 1, child.Length - 1);
            }
            else
            {
                builder.Append(child);
            }

            return builder.ToString();
        }

        protected virtual void MoveItem(string path, string destination) { throw new NotImplementedException(); }
        protected virtual object MoveItemDynamicParameters(string path, string destination) { throw new NotImplementedException(); }
        protected virtual string NormalizeRelativePath(string path, string basePath) { throw new NotImplementedException(); }

        // internals
        //internal string GetChildName(string path, System.Management.Automation.CmdletProviderContext context);
        //internal string GetParentPath(string path, string root, System.Management.Automation.CmdletProviderContext context);
        //internal bool IsItemContainer(string path, System.Management.Automation.CmdletProviderContext context);
        //internal string MakePath(string parent, string child, System.Management.Automation.CmdletProviderContext context);
        //internal void MoveItem(string path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal object MoveItemDynamicParameters(string path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal string NormalizeRelativePath(string path, string basePath, System.Management.Automation.CmdletProviderContext context);

        internal static string NormalizePath(string path)
        {
            return PathIntrinsics.NormalizePath(path);
        }
    }
}

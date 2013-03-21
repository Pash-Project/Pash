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
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class PathIntrinsics
    {
        private SessionStateGlobal _sessionState;

        internal PathIntrinsics(SessionStateGlobal sessionState)
        {
            _sessionState = sessionState;
        }

        public PathInfo CurrentFileSystemLocation { get; private set; }
        public PathInfo CurrentLocation { get { return _sessionState.CurrentLocation; } }

        public string Combine(string parent, string child)
        {
            return _sessionState.MakePath(parent, child);
        }

        public PathInfo CurrentProviderLocation(string providerName)
        {
            return _sessionState.CurrentProviderLocation(providerName);
        }

        public Collection<string> GetResolvedProviderPathFromProviderPath(string path, string providerId)
        {
            throw new NotImplementedException();
        }

        public Collection<string> GetResolvedProviderPathFromPSPath(string path, out ProviderInfo provider)
        {
            throw new NotImplementedException();
        }

        public Collection<PathInfo> GetResolvedPSPathFromPSPath(string path)
        {
            throw new NotImplementedException();
        }

        public string GetUnresolvedProviderPathFromPSPath(string path)
        {
            throw new NotImplementedException();
        }

        public string GetUnresolvedProviderPathFromPSPath(string path, out ProviderInfo provider, out PSDriveInfo drive)
        {
            throw new NotImplementedException();
        }

        public bool IsProviderQualified(string path)
        {
            throw new NotImplementedException();
        }

        public bool IsPSAbsolute(string path, out string driveName)
        {
            throw new NotImplementedException();
        }

        public bool IsValid(string path)
        {
            return _sessionState.IsValidPath(path);
        }

        public PathInfoStack LocationStack(string stackName)
        {
            return _sessionState.LocationStack(stackName);
        }

        public string NormalizeRelativePath(string path, string basePath)
        {
            return _sessionState.NormalizeRelativePath(path, basePath);
        }

        public string ParseChildName(string path)
        {
            return _sessionState.GetPathChildName(path);
        }

        public string ParseParent(string path, string root)
        {
            return _sessionState.GetParentPath(path, root);
        }

        public PathInfo PopLocation(string stackName)
        {
            return _sessionState.PopLocation(stackName);
        }

        public void PushCurrentLocation(string stackName)
        {
            _sessionState.PushCurrentLocation(stackName);
        }

        public PathInfoStack SetDefaultLocationStack(string stackName)
        {
            return _sessionState.SetDefaultLocationStack(stackName);
        }

        public PathInfo SetLocation(string path)
        {
            return _sessionState.SetLocation(path);
        }

        // internals
        //internal string Combine(string parent, string child, CmdletProviderContext context);
        //internal Collection<string> GetResolvedProviderPathFromProviderPath(string path, string providerId, CmdletProviderContext context);
        //internal Collection<string> GetResolvedProviderPathFromPSPath(string path, CmdletProviderContext context, out ProviderInfo provider);
        //internal Collection<PathInfo> GetResolvedPSPathFromPSPath(string path, CmdletProviderContext context);
        //internal string GetUnresolvedProviderPathFromPSPath(string path, CmdletProviderContext context, out ProviderInfo provider, out PSDriveInfo drive);
        //internal bool IsCurrentLocationOrAncestor(string path, CmdletProviderContext context);
        //internal bool IsValid(string path, CmdletProviderContext context);
        //internal string NormalizeRelativePath(string path, string basePath, CmdletProviderContext context);
        //internal string ParseChildName(string path, CmdletProviderContext context);
        //internal string ParseParent(string path, string root, CmdletProviderContext context);
        //internal PathInfo SetLocation(string path, CmdletProviderContext context);

        internal PathInfo SetLocation(string path, ProviderRuntime providerRuntime)
        {
            return _sessionState.SetLocation(path, providerRuntime);
        }

        #region Path Operations
        // TODO: make a common class that works with a path

        // TODO: reverse on Unix?
        public const char CorrectSlash = '\\';
        public const char WrongSlash = '/';

        internal static string NormalizePath(string path)
        {
            // TODO: should we normilize the path into a different direction on Unix?
            return path.Replace(WrongSlash, CorrectSlash);
        }

        public static string MakePath(string path, PSDriveInfo drive)
        {
            string format = "{0}:" + '\\' + "{1}";
            if (path.StartsWith("\\"))
            {
                format = "{0}:{1}";
            }
            return string.Format(format, new object[] { drive.Name, path });
        }

        public static string GetDriveFromPath(string path)
        {
            int iDelimiter = path.IndexOf(':');

            if (iDelimiter == -1)
                return null;

            return path.Substring(0, iDelimiter);
        }
        #endregion

        internal static bool IsAbsolutePath(string path, out string driveName)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path can't be null");
            }

            driveName = null;

            if (path.Length == 0)
            {
                return false;
            }

            if (path.StartsWith("."))
            {
                return false;
            }

            int index = path.IndexOf(":", StringComparison.CurrentCulture);

            if (index > 0)
            {
                driveName = path.Substring(0, index);
                return true;
            }

            return false;
        }

        internal static string RemoveDriveName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith(CorrectSlash.ToString()) || path.StartsWith(WrongSlash.ToString()) || path.StartsWith("."))
                return path;

            int index = path.IndexOf(":", StringComparison.CurrentCulture);

            if (index > 0)
            {
                index++;
                return path.Substring(index, path.Length - index);
            }

            return path;
        }

        internal static string GetFullProviderPath(ProviderInfo provider, string path)
        {
            if (provider == null)
            {
                throw new NullReferenceException("Provider can't be null");
            }
            if (path == null)
            {
                throw new NullReferenceException("Path can't be null");
            }

            string fullPath = path;
            bool hasProviderName = false;

            int index = path.IndexOf("::");
            if (index != -1)
            {
                string providerName = path.Substring(0, index);
                if (provider.IsNameMatch(providerName))
                {
                    hasProviderName = true;
                }
            }

            if (!hasProviderName)
            {
                fullPath = string.Format("{0}::{1}", provider.FullName, path);
            }

            return fullPath;
        }
    }
}

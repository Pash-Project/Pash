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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("FileSystem", ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    public sealed class FileSystemProvider : NavigationCmdletProvider, IContentCmdletProvider, IPropertyCmdletProvider, ISecurityDescriptorCmdletProvider
    {
        public const string ProviderName = "FileSystem";

        public FileSystemProvider()
        {
        }

        protected override void CopyItem(string path, string destinationPath, bool recurse)
        {
            throw new NotImplementedException();
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path can't be empty");
            }

            path = NormalizePath(path);

            DirectoryInfo directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                GetDirectoryContent(directory, recurse);
            }
            else
            {
                FileInfo item = new FileInfo(path);
                if (item.Exists)
                {
                    if ((item.Attributes & FileAttributes.Hidden) == 0)
                    {
                        WriteItemObject(item, path, false);
                    }
                }
                else
                {
                    Exception exception = new IOException("Path doesn't exist: " + path);
                    WriteError(new ErrorRecord(exception, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
                }
            }
        }

        private void GetDirectoryContent(DirectoryInfo directory, bool recurse)
        {
            DirectoryInfo[] directories = null;
            List<FileSystemInfo> list = new List<FileSystemInfo>();

            if (recurse)
            {
                directories = directory.GetDirectories();
            }

            // Get all the location-related items
            list.AddRange(directory.GetDirectories());
            list.AddRange(directory.GetFiles());

            foreach (FileSystemInfo info in list)
            {
                if ((info.Attributes & FileAttributes.Hidden) == 0)
                {
                    if (info is FileInfo)
                    {
                        WriteItemObject(info, info.FullName, false);
                    }
                    else
                    {
                        WriteItemObject(info, info.FullName, true);
                    }
                }
            }

            if (recurse && (directories != null))
            {
                foreach (DirectoryInfo dir in directories)
                {
                    if ((dir.Attributes & FileAttributes.Hidden) == 0)
                    {
                        GetDirectoryContent(dir, recurse);
                    }
                }
            }
        }

        protected override string GetChildName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            path = PathIntrinsics.NormalizePath(path);
            path = path.TrimEnd('\\');

            int num = path.LastIndexOf('\\');
            if (num == -1)
            {
                return MakeSlashedPath(path);
            }
            return path.Substring(num + 1);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            throw new NotImplementedException();
        }

        protected override void GetItem(string path)
        {
            bool isContainer = false;
            FileSystemInfo item = GetFileSystemInfo(path, ref isContainer, false);
            WriteItemObject(item, item.FullName, isContainer);
        }

        protected override string GetParentPath(string path, string root)
        {
            string parentPath = base.GetParentPath(path, root);

            // TODO: deal with UNC
            if (!path.StartsWith("\\\\")) // UNC?
            {
                parentPath = MakeSlashedPath(parentPath);
            }
            return parentPath;
        }

        private string MakeSlashedPath(string path)
        {
            // Make sure that the path is ended bith '\'
            int index = path.IndexOf(':');
            if ((index != -1) && ((index + 1) == path.Length))
            {
                path += '\\';
            }
            return path;
        }

        protected override bool HasChildItems(string path) { throw new NotImplementedException(); }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();

            // TODO: Console.WriteLine("Mono: Before GetDrives");
            DriveInfo[] drives = DriveInfo.GetDrives();
            // TODO: Console.WriteLine("Mono: After GetDrives");

            System.Diagnostics.Debug.WriteLine("Number of drives: " + ((drives == null) ? "Null drives" : drives.Length.ToString()));

            if (drives != null)
            {
                foreach (DriveInfo driveInfo in drives)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("drive '{0}' type '{1}' root '{2}'", driveInfo.Name, driveInfo.DriveType.ToString(), driveInfo.RootDirectory));

                    // Support for Mono names - there are no ':' (colon) in the name
                    string name = driveInfo.Name;
                    if (driveInfo.Name.IndexOf(':') > 0)
                        name = driveInfo.Name.Substring(0, 1);

                    string description = string.Empty;
                    string root = driveInfo.Name;
                    if (driveInfo.DriveType == DriveType.Fixed)
                    {
                        try
                        {
                            description = driveInfo.VolumeLabel;
                            root = driveInfo.RootDirectory.FullName;
                        }
                        catch
                        {
                        }
                    }
                    PSDriveInfo info = new PSDriveInfo(name, base.ProviderInfo, root, description, null);
                    info.RemovableDrive = driveInfo.DriveType != DriveType.Fixed;

                    collection.Add(info);
                }
            }
            return collection;
        }

        protected override void InvokeDefaultAction(string path) { throw new NotImplementedException(); }

        protected override bool IsItemContainer(string path)
        {
            path = NormalizePath(path);

            return (new DirectoryInfo(path)).Exists;
        }

        protected override bool IsValidPath(string path) { throw new NotImplementedException(); }

        protected override bool ItemExists(string path)
        {
            path = NormalizePath(path);
            try
            {
                FileInfo info = new FileInfo(path);
                if (info.Exists)
                {
                    return true;
                }
                else
                {
                    return (new DirectoryInfo(path)).Exists;
                }
            }
            catch
            {
            }

            return false;
        }

        protected override void MoveItem(string path, string destination) { throw new NotImplementedException(); }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            try
            {
                DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(drive.Root));
                if (driveInfo.DriveType == DriveType.Fixed)
                {
                    return (ItemExists(drive.Root) && IsItemContainer(drive.Root)) ? drive : null;
                }

                return drive;
            }
            catch
            {
            }

            return null;
        }

        protected override void NewItem(string path, string type, object value) { throw new NotImplementedException(); }

        protected override string NormalizeRelativePath(string path, string basePath) { throw new NotImplementedException(); }

        protected override void RemoveItem(string path, bool recurse) { throw new NotImplementedException(); }

        protected override void RenameItem(string path, string newName) { throw new NotImplementedException(); }

        protected override ProviderInfo Start(ProviderInfo providerInfo)
        {
            if ((providerInfo != null) && string.IsNullOrEmpty(providerInfo.Home))
            {
                string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
                string homePath = Environment.GetEnvironmentVariable("HOMEPATH");
                if (!string.IsNullOrEmpty(homeDrive) && !string.IsNullOrEmpty(homePath))
                {
                    string path = MakePath(homeDrive, homePath);
                    if (Directory.Exists(path))
                    {
                        providerInfo.Home = path;
                    }
                }
            }
            return providerInfo;
        }

        #region IContentCmdletProvider Members

        public void ClearContent(string path)
        {
            throw new NotImplementedException();
        }

        public object ClearContentDynamicParameters(string path)
        {
            return null;
        }

        public IContentReader GetContentReader(string path)
        {
            throw new NotImplementedException();
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return new FileSystemContentReaderDynamicParameters();
        }

        public IContentWriter GetContentWriter(string path)
        {
            throw new NotImplementedException();
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            return new FileSystemContentWriterDynamicParameters();
        }

        #endregion

        #region IPropertyCmdletProvider Members

        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            throw new NotImplementedException();
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        {
            throw new NotImplementedException();
        }

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            throw new NotImplementedException();
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            throw new NotImplementedException();
        }

        public void SetProperty(string path, PSObject propertyValue)
        {
            throw new NotImplementedException();
        }

        public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISecurityDescriptorCmdletProvider Members

        public void GetSecurityDescriptor(string path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
        {
            throw new NotImplementedException();
        }

        #endregion

        private FileSystemInfo GetFileSystemInfo(string path, ref bool isContainer, bool showHidden)
        {
            path = NormalizePath(path);
            FileInfo fi = new FileInfo(path);
            if (fi.Exists && ((fi.Attributes & FileAttributes.Directory) == 0))
            {
                return fi;
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    isContainer = true;
                    return di;
                }
            }
            return null;
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security;
using System.Security.AccessControl;
using System.Management;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("FileSystem", ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    public sealed class FileSystemProvider :
        NavigationCmdletProvider,
        IContentCmdletProvider,
        IPropertyCmdletProvider,
        ISecurityDescriptorCmdletProvider
    {
        public const string ProviderName = "FileSystem";

        public FileSystemProvider()
        {
        }

        protected override void CopyItem(Path path, Path destinationPath, bool recurse)
        {
            throw new NotImplementedException();
        }

        protected override void GetChildItems(Path path, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path can't be empty");
            }

            path = NormalizePath(path);

            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);
            if (directory.Exists)
            {
                GetDirectoryContent(directory, recurse);
            }
            else
            {
                System.IO.FileInfo item = new System.IO.FileInfo(path);
                if (item.Exists)
                {
                    if ((item.Attributes & System.IO.FileAttributes.Hidden) == 0)
                    {
                        WriteItemObject(item, path, false);
                    }
                }
                else
                {
                    Exception exception = new System.IO.IOException("Path doesn't exist: " + path);
                    WriteError(new ErrorRecord(exception, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
                }
            }
        }

        private void GetDirectoryContent(System.IO.DirectoryInfo directory, bool recurse)
        {
            System.IO.DirectoryInfo[] directories = null;
            var list = new List<System.IO.FileSystemInfo>();

            if (recurse)
            {
                directories = directory.GetDirectories();
            }

            // Get all the location-related items
            list.AddRange(directory.GetDirectories());
            list.AddRange(directory.GetFiles());

            foreach (System.IO.FileSystemInfo info in list)
            {
                if ((info.Attributes & System.IO.FileAttributes.Hidden) == 0)
                {
                    if (info is System.IO.FileInfo)
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
                foreach (var dir in directories)
                {
                    if ((dir.Attributes & System.IO.FileAttributes.Hidden) == 0)
                    {
                        GetDirectoryContent(dir, recurse);
                    }
                }
            }
        }

        protected override Path GetChildName(Path path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            return path.GetChildNameOrSelfIfNoChild();
            //
            //            path = PathIntrinsics.NormalizePath(path);
            //            path = path.TrimEnd('\\');
            //
            //            int num = path.LastIndexOf('\\');
            //            if (num == -1)
            //            {
            //TODO: what is this MakeSlashedPath and should the above GetChildNameOrSelfIfNoChild do somthing like that?
            //                return MakeSlashedPath(path);
            //            }
            //            return path.Substring(num + 1);
        }

        protected override void GetChildNames(Path path, ReturnContainers returnContainers)
        {
            throw new NotImplementedException();
        }

        protected override void GetItem(Path path)
        {
            bool isContainer = false;
            var fileSystemInfo = GetFileSystemInfo(path, ref isContainer, false);
            WriteItemObject(fileSystemInfo, fileSystemInfo.FullName, isContainer);
        }

        protected override Path GetParentPath(Path path, Path root)
        {
            Path parentPath = base.GetParentPath(path, root);

            // TODO: deal with UNC
            if (!path.StartsWith("\\\\")) // UNC?
            {
                parentPath = MakeSlashedPath(parentPath);
            }
            return parentPath;
        }

        private Path MakeSlashedPath(Path path)
        {
            // Make sure that the path is ended bith '\'
            int index = path.IndexOf(':');
            if ((index != -1) && ((index + 1) == path.Length))
            {
                path += '\\';
            }
            return path;
        }

        protected override bool HasChildItems(Path path) { throw new NotImplementedException(); }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();

            // TODO: Console.WriteLine("Mono: Before GetDrives");
            var drives = System.IO.DriveInfo.GetDrives();
            // TODO: Console.WriteLine("Mono: After GetDrives");

            System.Diagnostics.Debug.WriteLine("Number of drives: " + ((drives == null) ? "Null drives" : drives.Length.ToString()));

            if (drives != null)
            {
                foreach (System.IO.DriveInfo driveInfo in drives)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("drive '{0}' type '{1}' root '{2}'", driveInfo.Name, driveInfo.DriveType.ToString(), driveInfo.RootDirectory));

                    // Support for Mono names - there are no ':' (colon) in the name
                    Path name = driveInfo.Name;
                    if (driveInfo.Name.IndexOf(':') > 0)
                        name = driveInfo.Name.Substring(0, 1);

                    Path description = string.Empty;
                    Path root = driveInfo.Name;
                    if (driveInfo.DriveType == System.IO.DriveType.Fixed)
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
                    info.RemovableDrive = driveInfo.DriveType != System.IO.DriveType.Fixed;

                    collection.Add(info);
                }
            }
            return collection;
        }

        protected override void InvokeDefaultAction(Path path) { throw new NotImplementedException(); }

        protected override bool IsItemContainer(Path path)
        {
            path = NormalizePath(path);

            return (new System.IO.DirectoryInfo(path)).Exists;
        }

        protected override bool IsValidPath(Path path) { throw new NotImplementedException(); }

        protected override bool ItemExists(Path path)
        {
            path = NormalizePath(path);
            try
            {
                var info = new System.IO.FileInfo(path);
                if (info.Exists)
                {
                    return true;
                }
                else
                {
                    return (new System.IO.DirectoryInfo(path)).Exists;
                }
            }
            catch
            {
            }

            return false;
        }

        protected override void MoveItem(Path path, Path destination) { throw new NotImplementedException(); }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            try
            {
                var driveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(drive.Root));
                if (driveInfo.DriveType == System.IO.DriveType.Fixed)
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

        protected override ProviderInfo Start(ProviderInfo providerInfo)
        {
            if ((providerInfo != null) && string.IsNullOrEmpty(providerInfo.Home))
            {
                Path homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
                Path homePath = Environment.GetEnvironmentVariable("HOMEPATH");
                if (!string.IsNullOrEmpty(homeDrive) && !string.IsNullOrEmpty(homePath))
                {
                    Path path = MakePath(homeDrive, homePath);
                    if (System.IO.Directory.Exists(path))
                    {
                        providerInfo.Home = path;
                    }
                }
            }
            return providerInfo;
        }

        #region IContentCmdletProvider Members

        public void ClearContent(Path path)
        {
            throw new NotImplementedException();
        }

        public object ClearContentDynamicParameters(Path path)
        {
            return null;
        }

        public IContentReader GetContentReader(Path path)
        {
            throw new NotImplementedException();
        }

        public object GetContentReaderDynamicParameters(Path path)
        {
            return new FileSystemContentReaderDynamicParameters();
        }

        public IContentWriter GetContentWriter(Path path)
        {
            throw new NotImplementedException();
        }

        public object GetContentWriterDynamicParameters(Path path)
        {
            return new FileSystemContentWriterDynamicParameters();
        }

        #endregion

        #region IPropertyCmdletProvider Members

        public void ClearProperty(Path path, Collection<string> propertyToClear)
        {
            throw new NotImplementedException();
        }

        public object ClearPropertyDynamicParameters(Path path, Collection<string> propertyToClear)
        {
            throw new NotImplementedException();
        }

        public void GetProperty(Path path, Collection<string> providerSpecificPickList)
        {
            throw new NotImplementedException();
        }

        public object GetPropertyDynamicParameters(Path path, Collection<string> providerSpecificPickList)
        {
            throw new NotImplementedException();
        }

        public void SetProperty(Path path, PSObject propertyValue)
        {
            throw new NotImplementedException();
        }

        public object SetPropertyDynamicParameters(Path path, PSObject propertyValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISecurityDescriptorCmdletProvider Members

        public void GetSecurityDescriptor(Path path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public ObjectSecurity NewSecurityDescriptorFromPath(Path path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public void SetSecurityDescriptor(Path path, ObjectSecurity securityDescriptor)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.IO.FileSystemInfo GetFileSystemInfo(Path path, ref bool isContainer, bool showHidden)
        {
            path = NormalizePath(path);
            var fi = new System.IO.FileInfo(path);
            if (fi.Exists && ((fi.Attributes & System.IO.FileAttributes.Directory) == 0))
            {
                return fi;
            }
            else
            {
                var di = new System.IO.DirectoryInfo(path);
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

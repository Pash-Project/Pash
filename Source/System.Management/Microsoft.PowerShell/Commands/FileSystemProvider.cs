// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security;
using System.Security.AccessControl;
using System.Management;
using System.Management.Pash.Implementation;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("FileSystem", ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    public sealed class FileSystemProvider :
        NavigationCmdletProvider,
        IContentCmdletProvider,
        IPropertyCmdletProvider,
        ISecurityDescriptorCmdletProvider
    {
        private enum ItemType {
            Unknown,
            Directory,
            File
        };

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

        protected override string GetChildName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            return new Path(path).GetChildNameOrSelfIfNoChild();
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

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            throw new NotImplementedException();
        }

        protected override void GetItem(string path)
        {
            bool isContainer = false;
            var fileSystemInfo = GetFileSystemInfo(path, ref isContainer, false);
            WriteItemObject(fileSystemInfo, fileSystemInfo.FullName, isContainer);
        }

        protected override string GetParentPath(string path, string root)
        {
            Path parentPath = base.GetParentPath(path, root);

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
            var drives = System.IO.DriveInfo.GetDrives();
            // TODO: Console.WriteLine("Mono: After GetDrives");

            System.Diagnostics.Debug.WriteLine("Number of drives: " + ((drives == null) ? "Null drives" : drives.Length.ToString()));

            // TODO: Resolve hack to get around Mono bug where System.IO.DriveInfo.GetDrives() returns a single blank drive.
            if (MonoHasBug11923())
            {
                PSDriveInfo info = new PSDriveInfo("/", base.ProviderInfo, "/", "Root", null);
                info.RemovableDrive = false;

                collection.Add(info);
                return collection;
            }

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

        // See: https://bugzilla.xamarin.com/show_bug.cgi?id=11923
        static bool MonoHasBug11923()
        {
            var drives = System.IO.DriveInfo.GetDrives();
            return drives.Length == 1 && drives[0].Name.Length == 0;
        }

        protected override void InvokeDefaultAction(string path) { throw new NotImplementedException(); }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            path = NormalizePath(path);
            var type = GetItemType(itemTypeName);
            if (type.Equals(ItemType.Unknown))
            {
                throw new PSInvalidOperationException("Cannot create an item of unknown type");
            }
            System.IO.FileMode mode = System.IO.FileMode.CreateNew;
            if (Force.IsPresent)
            {
                mode = System.IO.FileMode.Create;
                CreateIntermediateDirectories(path);
            }
            if (type.Equals(ItemType.Directory))
            {
                var dirinfo = new System.IO.DirectoryInfo(path.ToString());
                dirinfo.Create();
                WriteItemObject(dirinfo, path, true);
                return;
            }
            // else ItemType is File
            if (!ShouldProcess(path))
            {
                return;
            }
            try
            {
                using (var stream = new System.IO.FileStream(path, mode, System.IO.FileAccess.Write, System.IO.FileShare.None))
                {
                    if (newItemValue != null)
                    {
                        var writer = new System.IO.StreamWriter(stream);
                        writer.Write(newItemValue.ToString());
                        writer.Flush();
                        writer.Close();
                    }
                }
                WriteItemObject(new System.IO.FileInfo(path), path, false);
            }
            catch (System.IO.IOException ex)
            {
                WriteError(new ErrorRecord(ex, "NewItem", ErrorCategory.WriteError, path));
            }
        }

        protected override bool IsItemContainer(string path)
        {
            path = NormalizePath(path);

            return (new System.IO.DirectoryInfo(path)).Exists;
        }

        protected override bool IsValidPath(string path) { throw new NotImplementedException(); }

        protected override bool ItemExists(string path)
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

        protected override void MoveItem(string path, string destination) { throw new NotImplementedException(); }

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

        void CreateIntermediateDirectories(string path)
        {
            var dirinfo = new System.IO.DirectoryInfo(path).Parent;
            var createStack = new Stack<System.IO.DirectoryInfo>();
            while (dirinfo != null && !dirinfo.Exists)
            {
                createStack.Push(dirinfo);
                dirinfo = dirinfo.Parent;
            }
            while (createStack.Count > 0)
            {
                dirinfo = createStack.Pop();
                dirinfo.Create();
            }
        }

        #region IContentCmdletProvider Members

        public void ClearContent(string path)
        {
            if (!ItemExists(path))
            {
                throw new ItemNotFoundException(string.Format("Cannot find path '{0}' because it does not exist.", path));
            }

            using (var writer = new System.IO.StreamWriter(path, false, Encoding.ASCII))
            {
            }
        }

        public object ClearContentDynamicParameters(string path)
        {
            return null;
        }

        public IContentReader GetContentReader(string path)
        {
            return new FileContentReader(path);
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

        private ItemType GetItemType(string type)
        {
            var pattern = new WildcardPattern(type + "*", WildcardOptions.IgnoreCase);
            if (pattern.IsMatch("directory") || pattern.IsMatch("container"))
            {
                return ItemType.Directory;
            }
            else if (pattern.IsMatch("file"))
            {
                return ItemType.File;
            }
            return ItemType.Unknown;
        }

        private System.IO.FileSystemInfo GetFileSystemInfo(string path, ref bool isContainer, bool showHidden)
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

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
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
        internal const string FallbackDriveName = "File";
        private enum ItemType {
            Unknown,
            Directory,
            File
        };

        public const string ProviderName = "FileSystem";

        public FileSystemProvider()
        {
        }

        protected override string NormalizeRelativePath(string path, string basePath)
        {
            var normPath = new Path(path).NormalizeSlashes();
            var normBase = new Path(basePath).NormalizeSlashes();
            if (!normPath.StartsWith(normBase))
            {
                var ex = new PSArgumentException("Path is outside of base path!", "PathOutsideBasePath",
                    ErrorCategory.InvalidArgument);
                WriteError(ex.ErrorRecord);
                return null;
            }

            return new Path(path.Substring(basePath.Length)).TrimStartSlash().ToString();
        }

        protected override void CopyItem(string path, string destinationPath, bool recurse)
        {
            throw new NotImplementedException();
        }

        protected override string MakePath(string parent, string child)
        {
            return new Path(parent).Combine(child).ToString();
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
            path = NormalizePath(path);
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
            if (path == String.Empty)
            {
                path = ".";
            }

            var names = GetChildNamesPrivate(path);
            foreach (var name in names)
            {
                ProviderRuntime.WriteObject(name);
            }
        }

        protected override void GetItem(string path)
        {
            bool isContainer = false;
            var fileSystemInfo = GetFileSystemInfo(path, ref isContainer, false);
            WriteItemObject(fileSystemInfo, fileSystemInfo.FullName, isContainer);
        }

        protected override string GetParentPath(string path, string root)
        {
            Path parentPath = new Path(path).GetParentPath(root);

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

        protected override bool HasChildItems(string path)
        {
            return GetChildNamesPrivate(path).Count > 0;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            /*
             * The concept of drives in Powershell is obviously inspired by the
             * Windows file system. However, in combination with providers, this concept
             * has much more power as it allows access to arbitrary data stores.
             * Handling paths is therefore a little bit more complicated, because we don't
             * just deal with the FS. As a consequence, we need to make sure that
             * drive qualified path can be identified with the colon and that drives have valid
             * names.
             * On non-Windows machines, mono would return "drives" like '/', '/proc', etc, because
             * that's what's closest to Windows-drives. However, we can't adopt these drives,
             * because their names are invalid and wouldn't be native on *nix systems with a colon.
             * So instead, if we have a platform with mono drives that don't include a colon itself,
             * and therefore aren't compatible to the PS/Pash drive concept, we will use a single
             * default drive called "File" (see FallbackDriveName) instead to uniquely access the
             * root of the file system.
             * This drive get's a hidden flag, so it will be ignored in various places when Pash
             * would for example display the path. Doing this should result in a feeling that is native
             * for the platform.
             */
            var fsDrives = System.IO.DriveInfo.GetDrives();
            var drives = (from fd in fsDrives
                          where fd.Name.Contains(":")
                          select NewDrive(fd)).ToList();
            if (drives.Count == 0)
            {
                var root = System.IO.Path.GetPathRoot(Environment.CurrentDirectory);
                var defaultDrive = new PSDriveInfo(FallbackDriveName, ProviderInfo, root, "Root", null);
                defaultDrive.Hidden = true;
                drives.Add(defaultDrive);
            }
            return new Collection<PSDriveInfo>(drives);
        }

        private PSDriveInfo NewDrive(System.IO.DriveInfo fsDrive)
        {
            Path name = fsDrive.Name;
            var iColon = fsDrive.Name.IndexOf(":");
            if (iColon > 0)
                name = fsDrive.Name.Substring(0, iColon);

            Path description = string.Empty;
            Path root = fsDrive.Name;
            if (fsDrive.DriveType == System.IO.DriveType.Fixed)
            {
                try
                {
                    description = fsDrive.VolumeLabel;
                    root = fsDrive.RootDirectory.FullName;
                }
                catch
                {
                }
            }
            PSDriveInfo info = new PSDriveInfo(name, base.ProviderInfo, root, description, null);
            info.RemovableDrive = fsDrive.DriveType != System.IO.DriveType.Fixed;
            return info;
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
            path = path == "" ? "." : path; // Workaround until our globber handles relative paths
            path = NormalizePath(path);

            return (new System.IO.DirectoryInfo(path)).Exists;
        }

        protected override bool IsValidPath(string path) { throw new NotImplementedException(); }

        protected override bool ItemExists(string path)
        {
            path = path == "" ? "." : path; // Workaround until our globber handles relative paths
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
            // The special drive for *nix systems is always valid
            if (drive.Name == FallbackDriveName &&
                drive.Root == System.IO.Path.GetPathRoot(Environment.CurrentDirectory))
            {
                return drive;
            }

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
            if (providerInfo == null || !string.IsNullOrEmpty(providerInfo.Home))
            {
                return providerInfo;
            }

            string path = null;
            Path homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            Path homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            if (!string.IsNullOrEmpty(homeDrive) && !string.IsNullOrEmpty(homePath))
            {
                path = MakePath(homeDrive, homePath);
            }
            else
            {
                // FIXME: taken from Path.ResolveTilde(). Make sure to clean all that mess up...
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // HACK: Use $Env:HOME until Mono 2.10 dies out.
                if (path == "")
                {
                    path = Environment.GetEnvironmentVariable("HOME");
                }
            }

            if (System.IO.Directory.Exists(path))
            {
                providerInfo.Home = path;
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
            path = NormalizePath(path);
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
            path = NormalizePath(path);
            return new FileContentReader(path);
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return new FileSystemContentReaderDynamicParameters();
        }

        public IContentWriter GetContentWriter(string path)
        {
            path = NormalizePath(path);
            return new FileContentWriter(path);
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

        internal string NormalizePath(string path)
        {
            var p = new Path(path).NormalizeSlashes();
            return p.ToString();
        }

        private List<string> GetChildNamesPrivate(string path)
        {
            path = path == "" ? "." : path; // Workaround until our globber handles relative paths
            path = NormalizePath(path);
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);
            if (!directory.Exists)
            {
                return new List<string> { new System.IO.FileInfo(path).Name };
            }
            return (from fs in directory.GetFileSystemInfos() select fs.Name).ToList();
        }

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

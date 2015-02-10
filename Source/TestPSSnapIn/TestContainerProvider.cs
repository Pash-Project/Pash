using System;
using System.Linq;
using System.Management.Automation.Provider;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace TestPSSnapIn
{
    public class TestContainerDrive : PSDriveInfo
    {
        public Dictionary<string, string> Items { get; private set; }
        public TestContainerDrive(PSDriveInfo info) : base(info)
        {
            Items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    [CmdletProvider(TestContainerProvider.ProviderName, ProviderCapabilities.ExpandWildcards)]
    public class TestContainerProvider : ContainerCmdletProvider
    {
        public const string ProviderName = "TestContainerProvider";
        public const string DefaultDriveName = "TestContainerItems";
        public const string DefaultDriveRoot = "/def/";
        public const string DefaultDrivePath = DefaultDriveName + ":/";
        public const string DefaultItemName = "defItem";
        public const string DefaultItemPath = DefaultDrivePath + DefaultItemName;
        public const string DefaultItemValue = "defValue";

        public TestContainerDrive CurrentDrive
        {
            get
            {
                return PSDriveInfo as TestContainerDrive;
            }
        }

        #region container related

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            path = NormalizePath(path);
            copyPath = NormalizePath(copyPath);
            var value = CurrentDrive.Items[path];
            CurrentDrive.Items[copyPath] = value;
            WriteItemObject(value, copyPath, false);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            path = NormalizePath(path);
            if (path.Length == 0)
            {
                foreach (var pair in GetFilteredItems())
                {
                    WriteItemObject(pair.Value, pair.Key, false);
                }
                return;
            }
            GetItem(path);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            path = NormalizePath(path);
            if (path.Length == 0)
            {
                foreach (var key in GetFilteredItems().Keys)
                {
                    WriteItemObject(key, key, false);
                }
                return;
            }
            if (ItemExists(path))
            {
                WriteItemObject(path, path, false);
            }
        }

        protected override bool HasChildItems(string path)
        {
            path = NormalizePath(path);
            return path.Length == 0 && CurrentDrive.Items.Count > 0;
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            var strVal = (newItemValue as string) ?? "";

            if (itemTypeName.Equals("uppercase", StringComparison.OrdinalIgnoreCase))
            {
                strVal = strVal.ToUpper();
            }
            else if (!String.IsNullOrEmpty(itemTypeName))
            {
                throw new ArgumentException("The item type must be either 'uppercase' or nothing");
            }

            path = NormalizePath(path);

            if (CurrentDrive.Items.ContainsKey(path))
            {
                throw new ArgumentException("Item with path '" + path + "' already exists.");
            }
            CurrentDrive.Items[path] = strVal;
            WriteItemObject(strVal, path, false);
        }


        protected override void RemoveItem(string path, bool recurse)
        {
            path = NormalizePath(path);
            if (path.Length > 0)
            {
                CurrentDrive.Items.Remove(path);
                return;
            }
            // otherwise the whole drive
            if (recurse)
            {
                CurrentDrive.Items.Clear();
            }
        }

        #endregion

        #region item related

        protected override string[] ExpandPath(string path)
        {
            path = NormalizePath(path);
            var wildcard = new WildcardPattern(path, WildcardOptions.IgnoreCase);
            return (from name in GetFilteredItems().Keys
                    where wildcard.IsMatch(name)
                    select name).ToArray();
        }

        protected override bool IsValidPath(string path)
        {
            // path with '/' as delimiter and names consisting of alphanumeric chars. first and last delimiter are optional
            return Regex.IsMatch(path, "^/?[a-zA-Z0-9]+$");
        }

        protected override void RenameItem(string path, string newName)
        {
            path = NormalizePath(path);
            if (CurrentDrive.Items.ContainsKey(newName))
            {
                throw new ArgumentException("Item with name '" + newName + "' already exists");
            }
            var value = CurrentDrive.Items[path];
            CurrentDrive.Items[newName] = value;
            CurrentDrive.Items.Remove(path);
            WriteItemObject(value, newName, false);
        }

        protected override void GetItem(string path)
        {
            path = NormalizePath(path);
            if (path.Length == 0)
            {
                WriteItemObject(CurrentDrive, path, true);
                return;
            }
            if (CurrentDrive.Items.ContainsKey(path))
            {
                WriteItemObject(CurrentDrive.Items[path], path, false);
            }
        }

        protected override bool ItemExists(string path)
        {
            path = NormalizePath(path);
            if (path.Length == 0)
            {
                return true;
            }
            return CurrentDrive.Items.ContainsKey(path);
        }

        #endregion

        #region drive related

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (drive is TestContainerDrive)
            {
                return drive;
            }
            return new TestContainerDrive(drive);
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var defDrives = new Collection<PSDriveInfo>();
            var drive = new TestContainerDrive(new PSDriveInfo(DefaultDriveName, ProviderInfo, DefaultDriveRoot,
                "Default drive for testing container items", null));

            drive.Items[DefaultItemName] = DefaultItemValue;

            defDrives.Add(drive);
            return defDrives;
        }

        #endregion

        #region private helpers

        private Dictionary<string, string> GetFilteredItems()
        {
            // our custom filter understands c# regex
            var items = new Dictionary<string, string>(CurrentDrive.Items);
            if (String.IsNullOrEmpty(Filter))
            {
                return items;
            }
            var regex = new Regex(Filter, RegexOptions.IgnoreCase);
            foreach (var rm in items.Keys.Where(k => !regex.IsMatch(k)).ToArray())
            {
                items.Remove(rm);
            }
            return items;
        }

        private string NormalizePath(string path)
        {
            if (path.StartsWith(PSDriveInfo.Root))
            {
                path = path.Substring(PSDriveInfo.Root.Length);
            }
            path = path.Trim('\\', '/');
            return path;
        }

        #endregion
    }
}


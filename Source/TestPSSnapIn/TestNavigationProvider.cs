using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TestPSSnapIn
{
    [CmdletProvider(TestNavigationProvider.ProviderName, ProviderCapabilities.None)]
    public class TestNavigationProvider : NavigationCmdletProvider
    {
        public const string ProviderName = "TestNavigationProvider";
        public const string DefaultDriveName = "TestNavigationItems";
        public const string DefaultDriveRoot = "%def";
        public const string DefaultDrivePath = DefaultDriveName + ":/";
        public const string SecondDriveName = "TestNavigationAlternative";
        public const string SecondDriveRoot = "%alt";
        public const string SecondDrivePath = SecondDriveName + ":/";

        private const string _pathSeparator = "/";

        public static List<string> Messages = new List<string>();
        public static List<string> ExistingPaths = new List<string>()
        {
            "%def/foo/bar.txt",
            "%def/foo/baz.doc",
            "%def/foo/foo/bla.txt",
            "%def/bar.doc",
            "%def/bar/foo.txt",
            "%alt/foo/blub.doc",
        };

        #region navigation related

        protected override string GetChildName(string path)
        {
            return SplitPath(path).Last();
        }

        protected override string GetParentPath(string path, string root)
        {
            // documentation says: if that the parent path should be in the same root tree... whatever this practically means
            /*
            if (!String.IsNullOrEmpty(root) && !path.Contains(root))
            {
                return null;
            }
            */
            return String.Join(_pathSeparator, SplitPath(path).Reverse().Skip(1).Reverse());
        }

        protected override bool IsItemContainer(string path)
        {
            // check if child contains an extension. otherwise we treat it as a container
            Messages.Add("IsItemContainer " + path);
            return !SplitPath(path).Last().Contains(".");
        }

        protected override string MakePath(string parent, string child)
        {
            if (String.IsNullOrEmpty(parent))
            {
                return child;
            }
            if (parent.EndsWith(_pathSeparator))
            {
                parent = parent.Substring(0, parent.Length - _pathSeparator.Length);
            }
            if (child.StartsWith(_pathSeparator))
            {
                child = child.Substring(_pathSeparator.Length);
            }
            return parent + _pathSeparator + child;
        }

        protected override void MoveItem(string path, string destination)
        {
            Messages.Add("CopyItem " + path + " " + " " + destination);
        }

        protected override string NormalizeRelativePath(string path, string basePath)
        {
            Messages.Add("NormalizeRelativePath " + path + " " + " " + basePath);
            return path;
        }

        #endregion

        #region container related

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            Messages.Add("CopyItem " + path + " " + " " + copyPath + " " + recurse);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            Messages.Add("GetChildItems " + path + " " + recurse);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            Messages.Add("GetChildNames " + path + " " + returnContainers);
            foreach (var c in ChildNames(path))
            {
                WriteItemObject(c, path + _pathSeparator + c, !c.Contains("."));
            }
        }

        protected override bool HasChildItems(string path)
        {
            Messages.Add("HasChildItems " + path);
            return ChildNames(path).Length > 0;
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            Messages.Add("NewItem " + path + " " + itemTypeName + " " + newItemValue);
        }
            

        protected override void RemoveItem(string path, bool recurse)
        {
            Messages.Add("RemoveItem " + path + " " + recurse);
        }

        #endregion

        #region item related

        protected override bool IsValidPath(string path)
        {
            Messages.Add("IsValidPath " + path);
            return true;
        }

        protected override void RenameItem(string path, string newName)
        {
            Messages.Add("RenameItem " + path + " " + newName);
        }

        protected override void GetItem(string path)
        {
            Messages.Add("GetItem " + path);
        }

        protected override bool ItemExists(string path)
        {
            Messages.Add("ItemExists " + path);
            return Exists(path);
        }

        #endregion

        #region drive related

        private static bool Exists(string path)
        {
            return (from p in ExistingPaths where p.StartsWith(path) select p).Count() > 0;
        }

        private static string[] ChildNames(string path)
        {
            return (from p in ExistingPaths
                    where p.StartsWith(path) && p.Length > path.Length
                    select SplitPath(p.Substring(path.Length))[0]).ToArray();
        }

        private static string[] SplitPath(string path)
        {
            return path.Split(new [] { _pathSeparator }, StringSplitOptions.RemoveEmptyEntries);
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var defDrives = new Collection<PSDriveInfo>();
            var drive = new PSDriveInfo(DefaultDriveName, ProviderInfo, DefaultDriveRoot,
                "Default drive for testing navigation items", null);
            defDrives.Add(drive);

            drive = new PSDriveInfo(SecondDriveName, ProviderInfo, SecondDriveRoot,
                "Alternative drive for testing navigation items", null);
            defDrives.Add(drive);
            return defDrives;
        }

        #endregion

    }
}

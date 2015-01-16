using System;
using System.Linq;
using System.Management.Automation.Provider;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestPSSnapIn
{
    [CmdletProvider(TestItemProvider.ProviderName, ProviderCapabilities.ExpandWildcards)]
    public class TestItemProvider : ItemCmdletProvider
    {
        public const string ProviderName = "TestItemProvider";
        public const string DefaultDriveName = "testItems";

        public class ItemTestDrive : PSDriveInfo
        {
            internal Dictionary<string, string> Items { get; set; }
            public ItemTestDrive(PSDriveInfo driveInfo) : base(driveInfo)
            {
                Items = new Dictionary<string, string>();
            }
        }

        private ItemTestDrive _defaultDrive
        {
            get
            {
                return SessionState.Drive.GetAllForProvider(ProviderName).First() as ItemTestDrive;
            }
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var defDrive = new ItemTestDrive(new PSDriveInfo(DefaultDriveName, ProviderInfo, @"testItems:", "Test Item Drive", null));
            return new Collection<PSDriveInfo>(new[] { defDrive });
        }

        protected override void ClearItem(string path)
        {
            var driveless = GetPathWithoutDrive(path);
            _defaultDrive.Items.Remove(driveless);
        }

        protected override string[] ExpandPath(string path)
        {
            var driveless = GetPathWithoutDrive(path);
            var wildcard = new WildcardPattern(driveless, WildcardOptions.IgnoreCase);
            return (from i in _defaultDrive.Items.Keys
                             where wildcard.IsMatch(i)
                             select i).ToArray();
        }

        protected override void GetItem(string path)
        {
            var driveless = GetPathWithoutDrive(path);
            WriteItemObject(_defaultDrive.Items[driveless], path, false);
        }

        protected override void InvokeDefaultAction(string path)
        {
            SetItem(path + ":DEFAULT", "true");
        }

        protected override bool IsValidPath(string path)
        {
            return Regex.IsMatch(path, @"[a-zA-Z:\\]+");
        }

        protected override bool ItemExists(string path)
        {
            var driveless = GetPathWithoutDrive(path);
            return _defaultDrive.Items.ContainsKey(driveless);
        }

        protected override void SetItem(string path, object value)
        {
            var driveless = GetPathWithoutDrive(path);
            var strValue = value.ToString();
            _defaultDrive.Items[driveless] = strValue;
            WriteItemObject(strValue, path, false);
        }

        private string GetPathWithoutDrive(string path)
        {
            var providerPrefix = ProviderName + "::";
            var drivePrefix = _defaultDrive.Root;
            if (path.StartsWith(providerPrefix))
            {
                path = path.Substring(providerPrefix.Length);
            }
            if (!path.StartsWith(drivePrefix))
            {
                throw new PSInvalidOperationException("Path doesn't begin with the default test drive!");
            }
            return path;
        }
    }
}


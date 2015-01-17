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
        public const string DefaultItemName = "defItem";
        public const string DefaultItemValue = "defItemValue";

        public class ItemTestDrive : PSDriveInfo
        {
            internal Dictionary<string, string> Items { get; set; }
            public ItemTestDrive(PSDriveInfo driveInfo) : base(driveInfo)
            {
                Items = new Dictionary<string, string>() { { DefaultItemName, DefaultItemValue } };
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
                             select _defaultDrive.Name + ":" + i).ToArray();
        }

        protected override void GetItem(string path)
        {
            var driveless = GetPathWithoutDrive(path);
            if (_defaultDrive.Items.ContainsKey(driveless))
            {
                WriteItemObject(_defaultDrive.Items[driveless], path, false);
            }
        }

        protected override void InvokeDefaultAction(string path)
        {
            WriteItemObject("invoked!", path + ":DEFAULT", false);
        }

        protected override bool IsValidPath(string path)
        {
            return Regex.IsMatch(path, @"^[a-zA-Z:\\]+$");
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
            var drivePrefix = _defaultDrive.Name + ":";
            if (path.StartsWith(providerPrefix))
            {
                path = path.Substring(providerPrefix.Length);
            }
            if (!path.StartsWith(drivePrefix))
            {
                // we throw an exception here so tests fail if an akward path appears in this function in any test
                throw new PSInvalidOperationException("Path doesn't begin with the default test drive!");
            }
            return path.Substring(drivePrefix.Length);
        }
    }
}


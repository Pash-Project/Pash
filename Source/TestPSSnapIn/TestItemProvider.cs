using System;
using System.Linq;
using System.Management.Automation.Provider;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestPSSnapIn
{
    [CmdletProvider(TestItemProvider.ProviderName, ProviderCapabilities.ExpandWildcards | ProviderCapabilities.Credentials)]
    public class TestItemProvider : ItemCmdletProvider
    {
        public const string ProviderName = "TestItemProvider";
        public const string DefaultItemName = "defItem";
        public const string DefaultItemValue = "defItemValue";
        public const string DefaultDriveName = "itemDefaultDrive";
        public const string DefaultDrivePath = DefaultDriveName + ":\\";

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
            var defDrive = new ItemTestDrive(new PSDriveInfo(DefaultDriveName, ProviderInfo, "", "Test Item Drive", null));
            return new Collection<PSDriveInfo>(new[] { defDrive });
        }

        protected override void ClearItem(string path)
        {
            _defaultDrive.Items.Remove(path);
        }

        protected override string[] ExpandPath(string path)
        {
            var wildcard = new WildcardPattern(path, WildcardOptions.IgnoreCase);
            return (from i in _defaultDrive.Items.Keys
                             where wildcard.IsMatch(i)
                             select i).ToArray();
        }

        protected override void GetItem(string path)
        {
            if (_defaultDrive.Items.ContainsKey(path))
            {
                var value = _defaultDrive.Items[path];
                if (Credential != null)
                {
                    value += ", " + Credential.UserName;
                }
                WriteItemObject(value, path, false);
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
            return _defaultDrive.Items.ContainsKey(path);
        }

        protected override void SetItem(string path, object value)
        {
            var strValue = value.ToString();
            _defaultDrive.Items[path] = strValue;
            WriteItemObject(strValue, path, false);
        }
    }
}


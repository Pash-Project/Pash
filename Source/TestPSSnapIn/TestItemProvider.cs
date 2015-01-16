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

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var defdrive = new TestDrive(new PSDriveInfo(DefaultDriveName, ProviderInfo, "", "Test Item Drive", null));
            return new Collection<PSDriveInfo>(new [] { defdrive });
        }

        protected override void ClearItem(string path)
        {

            var drive = PSDriveInfo as ItemTestDrive;
            var driveless = GetPathWithoutDrive(path);
            drive.Items.Remove(driveless);
        }

        protected override string[] ExpandPath(string path)
        {
            var drive = PSDriveInfo as ItemTestDrive;
            var driveless = GetPathWithoutDrive(path);
            var wildcard = new WildcardPattern(driveless, WildcardOptions.IgnoreCase);
            return (from i in drive.Items.Keys
                             where wildcard.IsMatch(i)
                             select i).ToArray();
        }

        protected override void GetItem(string path)
        {
            var drive = PSDriveInfo as ItemTestDrive;
            var driveless = GetPathWithoutDrive(path);
            WriteItemObject(drive.Items[driveless], path, false);
        }

        protected override void InvokeDefaultAction(string path)
        {
            SetItem(path + ":DEFAULT", "true");
        }

        protected override bool IsValidPath(string path)
        {
            return Regex.IsMatch(path, @"[a-zA-Z:]+");
        }

        protected override bool ItemExists(string path)
        {
            var drive = PSDriveInfo as ItemTestDrive;
            var driveless = GetPathWithoutDrive(path);
            return drive.Items.ContainsKey(driveless);
        }

        protected override void SetItem(string path, object value)
        {
            var drive = PSDriveInfo as ItemTestDrive;
            var driveless = GetPathWithoutDrive(path);
            drive.Items[driveless] = value.ToString();
        }

        private string GetPathWithoutDrive(string path)
        {
            var driveWithSep = PSDriveInfo.Name + ":";
            if (path.StartsWith(driveWithSep))
            {
                path = path.Substring(driveWithSep.Length);
            }
            return path;
        }
    }
}


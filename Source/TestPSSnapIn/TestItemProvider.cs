using System;
using System.Management.Automation.Provider;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace TestPSSnapIn
{
    public class TestItemProvider : ItemCmdletProvider
    {
        public const string ProviderName = "TestItemProvider";
        public const string DefaultDriveName = "testItems";

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var defdrive = new TestDrive(new PSDriveInfo(DefaultDriveName, ProviderInfo, "/", "Test Item Drive", null));
            return new Collection<PSDriveInfo>(new [] { defdrive });
        }

        protected override void ClearItem(string path)
        {
            base.ClearItem(path);
        }

        protected override string[] ExpandPath(string path)
        {
            return base.ExpandPath(path);
        }

        protected override void GetItem(string path)
        {
            base.GetItem(path);
        }

        protected override void InvokeDefaultAction(string path)
        {
            base.InvokeDefaultAction(path);
        }

        protected override bool IsValidPath(string path)
        {
            throw new NotImplementedException();
        }

        protected override bool ItemExists(string path)
        {
            return base.ItemExists(path);
        }

        protected override void SetItem(string path, object value)
        {
            base.SetItem(path, value);
        }
    }
}


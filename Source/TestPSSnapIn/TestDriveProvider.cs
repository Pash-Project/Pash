using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Collections.ObjectModel;

namespace TestPSSnapIn
{
    public class TestDrive : PSDriveInfo
    {
        public bool IsRemoved { get; internal set; }
        public TestDrive(PSDriveInfo driveInfo) : base(driveInfo)
        {
            IsRemoved = false;
        }
    }

    public class TestProviderInfo : ProviderInfo
    {
        public bool IsStopped { get; internal set; }
        public TestProviderInfo(ProviderInfo providerInfo) : base(providerInfo)
        {
            IsStopped = false;
        }
    }

    [CmdletProvider(TestDriveProvider.ProviderName, ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    public class TestDriveProvider : DriveCmdletProvider
    {
        public const string ProviderName = "TestDriveProvider";
        public const string DefaultDriveName = "testDefault";

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            return new TestDrive(drive);
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            var testdrive = drive as TestDrive;
            if (drive == null)
            {
                throw new InvalidOperationException("Drive is not a TestDrive!");
            }
            testdrive.IsRemoved = true;
            return testdrive;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var defdrive = new TestDrive(new PSDriveInfo(DefaultDriveName, ProviderInfo, "/", "Test Default Drive", null));
            return new Collection<PSDriveInfo>(new [] { defdrive });
        }

        protected override ProviderInfo Start(ProviderInfo providerInfo)
        {
            return new TestProviderInfo(providerInfo);
        }

        protected override void Stop()
        {
            (ProviderInfo as TestProviderInfo).IsStopped = true;
        }

    }
}


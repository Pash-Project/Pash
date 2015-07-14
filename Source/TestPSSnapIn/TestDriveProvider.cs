using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Collections.ObjectModel;

namespace TestPSSnapIn
{
    public class TestDriveDynamicParameters
    {
        [Parameter]
        public string Note { get; set; }
    }

    public class TestDrive : PSDriveInfo
    {
        public bool IsRemoved { get; internal set; }
        public string Note { get; internal set; }
        public TestDrive(PSDriveInfo driveInfo, string note) : base(driveInfo)
        {
            IsRemoved = false;
            Note = note;
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
            var dynamicParameters = DynamicParameters as TestDriveDynamicParameters;
            return new TestDrive(drive, dynamicParameters == null ? null : dynamicParameters.Note);
        }

        protected override object NewDriveDynamicParameters()
        {
            return new TestDriveDynamicParameters();
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
            var defdrive = new PSDriveInfo(DefaultDriveName, ProviderInfo, "/", "Test Default Drive", null);
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


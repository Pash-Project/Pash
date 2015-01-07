using System;
using NUnit.Framework;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{
    // the auto-loaded test module also imports all test providers, so we can simply use them
    [TestFixture]
    public class DriveCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        [Test]
        public void DriveCmdletProviderCanCreateDefaultDrive()
        {
            var cmd = NewlineJoin(
                "$d = Get-PSDrive -PSProvider " + TestDriveProvider.ProviderName,
                "$d.Name", // should be only one
                "$d.Root" // is '/'
            );
            ExecuteAndCompareTypedResult(cmd, TestDriveProvider.DefaultDriveName, "/");
        }

        [Test]
        public void DriveCmdletProviderCanCreateDrive()
        {
            var cmd = NewlineJoin(
                "$d = New-PSDrive -Name 'testDrive' -Root '/test' -PSProvider " + TestDriveProvider.ProviderName,
                "[object].ReferenceEquals((Get-PSDrive -Name 'testDrive'), $d)", // make sure it's listed by Get-PSDrive
                "$d.GetType().FullName", // is a custom type
                "$d.Root", // check if correctly passed
                "$d.IsRemoved" // property of the custom type
            );
            ExecuteAndCompareTypedResult(cmd,
                true, // listed by Get-PSDrive
                typeof(TestDrive).FullName, // of correct custom type
                "/test", // with correct root
                false // correct value of custom property
            );
        }

        [Test]
        public void DriveCmdletProviderCanRemoveDrive()
        {
            var cmd = NewlineJoin(
                "$d = New-PSDrive -Name 'testDrive' -Root '/test' -PSProvider " + TestDriveProvider.ProviderName,
                "$d.IsRemoved", // property of the custom type is false by default
                "$r = Remove-PSDrive -Name 'testDri*' -PSProvider " + TestDriveProvider.ProviderName,
                "[object]::ReferenceEquals($d, $r)", // make sure it's the same object
                "$d.IsRemoved", // should be true now, because of uninitialization
                "Get-PSDrive -Name 'testDrive'" // should be null, as it's removed
            );
            ExecuteAndCompareTypedResult(cmd,
                false, // custom property is false
                true, // same object reference,
                true, // custom property is now true
                null // not listed anymore
            );
        }
    }
}


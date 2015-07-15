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
                "$d.Provider.GetType().FullName", // the custom provider info
                "$d.Root", // is '/'
                "$d.Note" // is null
            );
            ExecuteAndCompareTypedResult(cmd, TestDriveProvider.DefaultDriveName, typeof(TestProviderInfo).FullName, "/", null);
        }

        [Test]
        public void DriveCmdletProviderCanCreateDrive()
        {
            var cmd = NewlineJoin(
                "$d = New-PSDrive -Name 'testDrive' -Root '/test' -PSProvider " + TestDriveProvider.ProviderName,
                "[object]::ReferenceEquals((Get-PSDrive -Name 'testDrive'), $d)", // make sure it's listed by Get-PSDrive
                "$d.GetType().FullName", // is a custom type
                "$d.Root", // check if correctly passed
                "$d.IsRemoved", // property of the custom type
                "$d.Note" // unset dynamic parameter
            );
            ExecuteAndCompareTypedResult(cmd,
                true, // listed by Get-PSDrive
                typeof(TestDrive).FullName, // of correct custom type
                "/test", // with correct root
                false, // correct value of custom property
                null
            );
        }

        [Test]
        public void DriveCmdletProviderCanCreateDriveWithDynamicParameters()
        {
            var cmd = NewlineJoin(
                "$d = New-PSDrive -Name 'testDrive' -Root '/test' -PSProvider " + TestDriveProvider.ProviderName + " -Note 'Custom Note'",
                "[object]::ReferenceEquals((Get-PSDrive -Name 'testDrive'), $d)", // make sure it's listed by Get-PSDrive
                "$d.GetType().FullName", // is a custom type
                "$d.Root", // check if correctly passed
                "$d.IsRemoved", // property of the custom type
                "$d.Note"
            );
            ExecuteAndCompareTypedResult(cmd,
                true, // listed by Get-PSDrive
                typeof(TestDrive).FullName, // of correct custom type
                "/test", // with correct root
                false, // correct value of custom property
                "Custom Note"
            );
        }

        [Test]
        public void DriveCmdletProviderCanRemoveDrive()
        {
            var cmd = NewlineJoin(
                "$d = New-PSDrive -Name 'testDrive' -Root '/test' -PSProvider " + TestDriveProvider.ProviderName,
                "$d.IsRemoved", // property of the custom type is false by default
                "Remove-PSDrive -Name 'testDri*' -PSProvider " + TestDriveProvider.ProviderName,
                "$d.IsRemoved", // should be true now, because of uninitialization
                "(Get-PSDrive | where Name -eq 'testDrive') -eq $null" // should be true, because testDrive was removed
            );
            ExecuteAndCompareTypedResult(cmd,
                false, // custom property is false
                true, // custom property is now true
                true // not listed anymore
            );
        }
    }
}


using System;
using NUnit.Framework;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{
    [TestFixture]
    public class ProviderLoadingTests : ReferenceTestBase
    {
        [Test]
        public void ProviderFromModuleIsLoadedAndInitialized()
        {
            var cmd = NewlineJoin(
                "(Get-PSProvider | ? name -eq " + TestDriveProvider.ProviderName + ") -eq $null", // make sure it's not loaded
                "Import-Module '" + BinaryTestModule + "'",
                "$p = Get-PSProvider " + TestDriveProvider.ProviderName,
                "$p.ImplementingType.FullName" // to verify it's not empty and is the correct type
            );
            ExecuteAndCompareTypedResult(cmd,
                true, // not loaded at first
                typeof(TestDriveProvider).FullName // make sure the provider is loaded
            );
        }

        [Test, Ignore("When executed with PS 3.0 this doesn't work. The same commands work as a script."+
                      "I think PS doesn't get the connection between the provider and the module anymore when NUnit is used")]
        public void ProviderFromModuleCanBeRemovedAndUninitialized()
        {
            var cmd = NewlineJoin(
                "Import-Module '" + BinaryTestModule + "'",
                "$p = Get-PSProvider " + TestDriveProvider.ProviderName,
                "$p -eq $null", // false
                "Remove-Module " + BinaryTestModuleName,
                "(Get-PSProvider | ? name -eq " + TestDriveProvider.ProviderName + ") -eq $null"// should be true now
            );
            ExecuteAndCompareTypedResult(cmd, false, true);
        }
    }
}


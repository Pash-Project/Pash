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
                "Get-PSProvider " + TestDriveProvider.ProviderName, // make sure it's not loaded
                "Import-Module '" + BinaryTestModule + "'",
                "$p = Get-PSProvider " + TestDriveProvider.ProviderName,
                "$p.ImplementingType.FullName", // to verify it's not empty and is the correct type
                "$p.IsStopped" // query a property which is from the custom TestProviderInfo
            );
            ExecuteAndCompareTypedResult(cmd,
                null, // not loaded at first
                typeof(TestDriveProvider).FullName, // make sure the provider is loaded
                false // the custom property
            );
        }

        [Test]
        public void ProviderFromModuleCanBeRemovedAndUninitialized()
        {
            var cmd = NewlineJoin(
                "Import-Module '" + BinaryTestModule + "'",
                "$p = Get-PSProvider " + TestDriveProvider.ProviderName,
                "$p.IsStopped", // verify that this custom property is false
                "Remove-Module " + BinaryTestModuleName,
                "$p.IsStopped", // should be true now because of unitialization
                "Get-PSProvider " + TestDriveProvider.ProviderName // should be null now
            );
            ExecuteAndCompareTypedResult(cmd,
                false, // custom value is false first
                true, // custom value is true after uninitialization
                null // provider isn't present anymore
            );
        }
    }
}


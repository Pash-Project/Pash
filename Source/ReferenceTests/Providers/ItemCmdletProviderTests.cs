using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{   
    [TestFixture]
    public class ItemCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        private readonly string _fullDrivePrefix = TestItemProvider.ProviderName + "::" + TestItemProvider.DefaultDriveName + ":";

        [TestCase("foo:bar", true)]
        [TestCase("foo/bar?baz", false)] // TestItemProvider only allows a-z, \, and :
        public void ItemProviderCanValidatePath(string path, bool valid)
        {
            var cmd = "Test-Path -IsValid '" + TestItemProvider.DefaultDriveName + ":" + path + "'";
            ExecuteAndCompareTypedResult(cmd, valid);
        }

        [Test]
        public void ItemProviderCanSetItem()
        {
            var cmd = "Set-Item -Path '" + _fullDrivePrefix + "foo' -Value 'bar' -PassThru";
            ExecuteAndCompareTypedResult(cmd, "bar");
        }
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{
    [TestFixture]
    class ContainerCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        [Test]
        public void ContainerProviderSupportsGetItem()
        {
            var cmd = "(Get-Item " + TestContainerProvider.DefaultItemPath + ").Value";
            ExecuteAndCompareTypedResult(cmd, TestContainerProvider.DefaultItemValue);
        }

        [Test]
        public void ContainerProviderSupportsGetItemWithNode()
        {
            var cmd = "Get-Item " + TestContainerProvider.DefaultNodePath;
            ExecuteAndCompareType(cmd, typeof(TestTreeNode));
        }

        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "any", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "leaf", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "container", false)]
        [TestCase(TestContainerProvider.DefaultItemPath, "leaf", true)]
        [TestCase(TestContainerProvider.DefaultItemPath, "container", false)]
        [TestCase(TestContainerProvider.DefaultItemPath, "any", true)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "leaf", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "container", true)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "any", true)]
        [TestCase(TestContainerProvider.DefaultNodePath, "any", true)]
        [TestCase(TestContainerProvider.DefaultNodePath, "leaf", true)] // is technically a container, but ContainerProvider only supports drives as containers
        [TestCase(TestContainerProvider.DefaultNodePath, "container", false)] // is technically a container, but ContainerProvider only supports drives as containers
        public void ContainerProviderSupportsTestPath(string path, string type, bool expected)
        {
            var cmd = "Test-Path " + path + " -PathType " + type;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [TestCase("newLeaf2", "leaf", "testValue")]
        [TestCase("newNode", "node", null)]
        public void ContainerProviderSupportsNewItem(string path, string type, string value)
        {
            path = TestContainerProvider.DefaultDrivePath + path;
            var psValue = value == null ? "$null" : "'" + value + "'";
            var cmd = NewlineJoin(
                "$ni = New-Item " + path + " -ItemType " + type + " -Value " + psValue,
                "$gi = Get-Item " + path,
                "[object]::ReferenceEquals($ni, $gi)",
                "$gi.Value"
            );
            ExecuteAndCompareTypedResult(cmd, true, value);
        }

        [TestCase("node", null)] // custom type of provider
        [TestCase("leaf", "leafValue")]
        public void ContainerProviderSupportsNewItemByName(string type, string value)
        {
            var psValue = value == null ? "" : " -Value '" + value + "'";
            var cmd = NewlineJoin(
                "$ni = New-Item " + TestContainerProvider.DefaultDrivePath + "foo/bar/baz -Name new -ItemType " + type + psValue,
                "$gi = Get-Item " + TestContainerProvider.DefaultDrivePath + "new", // important: ContainerProvider ignores the path if name is provided
                "[object]::ReferenceEquals($ni, $gi)",
                "$gi.Value"
            );
            ExecuteAndCompareTypedResult(cmd, true, value);
        }

        [Test]
        public void ContainerProviderThrowsOnNewItemWithInvalidType()
        {
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                // "container" is not a valid type for out test provider
                ReferenceHost.Execute("New-Item " + TestContainerProvider.DefaultDrivePath + "foo -ItemType 'container'");
            });
        }

        [Test]
        public void ContainerProviderThrowsOnNewItemAlreadyExists()
        {
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                ReferenceHost.Execute("New-Item " + TestContainerProvider.DefaultItemPath + " -ItemType 'leaf'");
            });
        }

        [Test]
        public void ContainerProviderThrowsOnNewItemInvalidPath()
        {
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                // nonExisting is a not existing parent, so item creation fails here
                ReferenceHost.Execute("New-Item " + TestContainerProvider.DefaultDrivePath + "notExisting/foo -ItemType 'leaf'");
            });
        }

        [Test]
        public void ContainerProviderSupportsRemoveItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsRemoveItemWithRecursion()
        {

        }

        [Test]
        public void ContainerProviderSupportsRemoveItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsRenameItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsRenameItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsCopyItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsCopyItemWithRecursion()
        {

        }

        [Test]
        public void ContainerProviderSupportsCopyItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetChildItem()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetChildItemWithRecursion()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetChildItemWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsGetLocation()
        {

        }

        [Test]
        public void ContainerProviderSupportsSetLocation()
        {

        }

        [Test]
        public void ContainerProviderSupportsSetLocationWithError()
        {

        }

        [Test]
        public void ContainerProviderSupportsPushLocation()
        {

        }

        [Test]
        public void ContainerProviderSupportsPopLocation()
        {

        }

    }
}

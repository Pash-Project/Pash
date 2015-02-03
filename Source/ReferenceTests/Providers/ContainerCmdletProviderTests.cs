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

        [TestCase(TestContainerProvider.DefaultDrivePath + "newLeaf1", "leaf", "testValue")]
        [TestCase(TestContainerProvider.DefaultDrivePath + "newNode", "node", null)]
        [TestCase(TestContainerProvider.DefaultNodePath + "newLeaf2", "leaf", "testValue2")]
        public void ContainerProviderSupportsNewItem(string path, string type, string value)
        {
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

        [TestCase(TestContainerProvider.DefaultItemPath)]
        [TestCase(TestContainerProvider.DefaultNodePath)]
        public void ContainerProviderSupportsRemoveItem(string path)
        {
            var cmd = "Remove-Item " + path;
            Assert.DoesNotThrow(delegate {
                ReferenceHost.Execute(cmd);
            });
        }

        [Test]
        public void ContainerProviderSupportsRemoveItemWithRecursion()
        {
            var parentPath = TestContainerProvider.DefaultNodePath;
            var childPath = parentPath + "testItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + childPath + " -ItemType leaf -Value 'testValue'",
                "Test-Path " + childPath,
                "Test-Path " + parentPath,
                "Remove-Item " + parentPath + " -Recurse",
                "Test-Path " + childPath,
                "Test-Path " + parentPath
            );
            ExecuteAndCompareTypedResult(cmd, true, true, false, false);
        }

        [Test]
        public void ContainerProviderThrowsOnRemoveItemNotExisting()
        {
            var cmd = "Remove-Item " + TestContainerProvider.DefaultDrivePath + "notExisting";
            Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute(cmd);
            });
        }

        [Test]
        public void ContainerProviderThrowsOnRemoveNodeWithoutRecursion()
        {
            var parentPath = TestContainerProvider.DefaultNodePath;
            var childPath = parentPath + "testItem";
            ReferenceHost.Execute("New-Item " + childPath + " -ItemType leaf -Value 'testValue'");
            var cmd = "Remove-Item " + parentPath;
            Assert.Throws<CmdletInvocationException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace(cmd);
            });
        }

        [Test]
        public void ContainerProviderSupportsRenameItem()
        {
            var itemPath = TestContainerProvider.DefaultItemPath;
            var newPath = TestContainerProvider.DefaultDrivePath + "newName";
            var cmd = NewlineJoin(
                "Test-Path " + itemPath, // exists
                "Test-Path " + newPath, // doesn't
                "Rename-Item " + itemPath + " -NewName newName",
                "Test-Path " + itemPath, // doesn't exist anymore
                "Test-Path " + newPath // should eixt now
            );
            ExecuteAndCompareTypedResult(cmd, true, false, false, true);
        }

        [Test]
        public void ContainerProviderSupportsRenameItemWithChildren()
        {
            var nodePath = TestContainerProvider.DefaultNodePath;
            var itemPath = TestContainerProvider.DefaultNodePath + "someItem";
            var newPath = TestContainerProvider.DefaultDrivePath + "newNodeName";
            var newItemPath = newPath + "/someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + itemPath + " -ItemType 'leaf' -Value 'testValue'",
                "Test-Path " + nodePath, // exists
                "Test-Path " + itemPath, // was creted before
                "Test-Path " + newPath, // should exist
                "Test-Path " + newItemPath, // should exist
                "$ri = Rename-Item " + nodePath + " -NewName newNodeName -PassThru",
                "Test-Path " + nodePath, // was renamed
                "Test-Path " + itemPath, // child exists under new name now
                "Test-Path " + newPath, // renamed node
                "Test-Path " + newItemPath, // same child, under new parent
                "[object]::ReferenceEquals($ni.Parent, $ri)" // still same item
            );
            ExecuteAndCompareTypedResult(cmd, true, true, false, false, false, false, true, true, true);
        }

        [Test]
        public void ContainerProviderThrowsOnRenameItemWithWithExistingDestination()
        {
            var cmd = "Rename-Item " + TestContainerProvider.DefaultItemPath + " -NewName " +  TestContainerProvider.DefaultNodeName;
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                ReferenceHost.Execute(cmd);
            });
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

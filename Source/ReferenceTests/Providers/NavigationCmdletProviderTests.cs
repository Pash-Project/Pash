using System;
using System.Linq;
using NUnit.Framework;
using TestPSSnapIn;
using System.Management.Automation;

namespace ReferenceTests.Providers
{
    public class NavigationCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "any", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "leaf", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath + "notExisting", "container", false)]
        [TestCase(TestContainerProvider.DefaultItemPath, "leaf", true)]
        [TestCase(TestContainerProvider.DefaultItemPath, "container", false)]
        [TestCase(TestContainerProvider.DefaultItemPath, "any", true)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "leaf", false)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "container", true)]
        [TestCase(TestContainerProvider.DefaultDrivePath, "any", true)]
        [TestCase(TestNavigationProvider.DefaultNodePath, "any", true)]
        [TestCase(TestNavigationProvider.DefaultNodePath, "leaf", false)]
        [TestCase(TestNavigationProvider.DefaultNodePath, "container", true)]
        public void NavigationProviderSupportsTestPath(string path, string type, bool expected)
        {
            var cmd = "Test-Path " + path + " -PathType " + type;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void NavigationProviderSupportsGetItemWithNode()
        {
            var cmd = "Get-Item " + TestNavigationProvider.DefaultNodePath;
            ExecuteAndCompareType(cmd, typeof(TestTreeNode));
        }

        [TestCase(TestNavigationProvider.DefaultDrivePath + "newLeaf1", "leaf", "testValue")]
        [TestCase(TestNavigationProvider.DefaultDrivePath + "newNode", "node", null)]
        [TestCase(TestNavigationProvider.DefaultNodePath + "newLeaf2", "leaf", "testValue2")]
        public void NavigationProviderSupportsNewItem(string path, string type, string value)
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


        [Test]
        public void NavigationProviderThrowsOnNewItemInvalidPath()
        {
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                // nonExisting is a not existing parent, so item creation fails here
                ReferenceHost.Execute("New-Item " + TestNavigationProvider.DefaultDrivePath + "notExisting/foo -ItemType 'leaf' -Value 'x'");
            });
        }

        [TestCase(TestNavigationProvider.DefaultItemPath)]
        [TestCase(TestNavigationProvider.DefaultNodePath)]
        public void NavigationProviderSupportsRemoveItem(string path)
        {
            var cmd = NewlineJoin(
                "Test-Path " + path,
                "Remove-Item " + path,
                "Test-Path " + path
            );
            ExecuteAndCompareTypedResult(cmd, true, false);
        }

        [Test]
        public void NavigationProviderSupportsRemoveItemWithRecursion()
        {
            var parentPath = TestNavigationProvider.DefaultNodePath;
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
        public void NavigationProviderThrowsOnRemoveNodeWithoutRecursion()
        {
            var parentPath = TestNavigationProvider.DefaultNodePath;
            var childPath = parentPath + "testItem";
            ReferenceHost.Execute("New-Item " + childPath + " -ItemType leaf -Value 'testValue'");
            var cmd = "Remove-Item " + parentPath;
            Assert.Throws<CmdletInvocationException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace(cmd);
            });
        }

        [Test]
        public void NavigationProviderSupportsRenameItemWithChildren()
        {
            var nodePath = TestNavigationProvider.DefaultNodePath;
            var itemPath = TestNavigationProvider.DefaultNodePath + "someItem";
            var newPath = TestNavigationProvider.DefaultDrivePath + "newNodeName";
            var newItemPath = newPath + "/someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + itemPath + " -ItemType 'leaf' -Value 'testValue'",
                "Test-Path " + nodePath, // exists
                "Test-Path " + itemPath, // was created before
                "Test-Path " + newPath, // shouldnt exist
                "Test-Path " + newItemPath, // shouldnt exist
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
        public void NavigationProviderSupportsCopyItemIntoSub()
        {
            var origPath = TestNavigationProvider.DefaultItemPath;
            var copyPath = TestNavigationProvider.DefaultNodePath + "copiedItem";
            var cmd = NewlineJoin(
                "Test-Path " + origPath,
                "Test-Path " + copyPath,
                "$ci = Copy-Item " + origPath + " " + copyPath + " -PassThru",
                "Test-Path " + origPath,
                "Test-Path " + copyPath,
                "$gi = Get-Item " + copyPath,
                "[object]::ReferenceEquals($ci, $gi)",
                "$ci.Value"
            );
            ExecuteAndCompareTypedResult(cmd, true, false, true, true, true, TestNavigationProvider.DefaultItemValue);
        }

        [Test]
        public void NavigationProviderSupportsGetChildItem()
        {
            var newPath = TestNavigationProvider.DefaultNodePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -ItemType 'leaf' -Value 'someValue'",
                "Get-ChildItem " + TestNavigationProvider.DefaultDrivePath + " | % { $_.Name }"
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultNodeName));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultItemName));
        }

        [Test]
        public void NavigationProviderSupportsGetChildItemNames()
        {
            var newPath = TestNavigationProvider.DefaultNodePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -ItemType 'leaf' -Value 'someValue'",
                "Get-ChildItem " + TestNavigationProvider.DefaultDrivePath + " -Name"
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultNodeName));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultItemName));
        }

        [Test]
        public void NavigationProviderSupportsGetChildItemWithRecursion()
        {
            var newPath = TestNavigationProvider.DefaultNodePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -ItemType 'leaf' -Value 'someValue'",
                "Get-ChildItem " + TestNavigationProvider.DefaultDrivePath + " -Recurse | % { $_.Name }"
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultNodeName));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultItemName));
            Assert.That(results, Contains.Item("someItem"));
        }

        [Test]
        public void NavigationProviderSupportsGetChildItemNamesWithRecursion()
        {
            var newPath = TestNavigationProvider.DefaultNodePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -ItemType 'leaf' -Value 'someValue'",
                "Get-ChildItem " + TestNavigationProvider.DefaultDrivePath + " -Recurse -Name"
            );

            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(3));
            // relative path, not absolute!
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultNodeName));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultItemName));
            Assert.That(results, Contains.Item(TestNavigationProvider.DefaultNodeName + "/" + "someItem"));
        }
    }
}


using System;
using System.Linq;
using NUnit.Framework;
using TestPSSnapIn;
using System.Management.Automation;
using System.Collections.Generic;

namespace ReferenceTests.Providers
{
    public class NavigationCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        private const string _defDrive = TestNavigationProvider.DefaultDrivePath;
        private const string _defRoot = TestNavigationProvider.DefaultDriveRoot;
        private const string _secDrive = TestNavigationProvider.SecondDrivePath;
        private const string _secRoot = TestNavigationProvider.SecondDriveRoot;

        void AssertMessagesAreEqual(params string[] expected)
        {
            CollectionAssert.AreEqual(expected, TestNavigationProvider.Messages);
        }

        void AssertMessagesContain(params string[] expected)
        {
            CollectionAssert.IsSubsetOf(expected, TestNavigationProvider.Messages);
        }


        void AssertMessagesDoesntContain(params string[] expected)
        {
            CollectionAssert.IsNotSubsetOf(expected, TestNavigationProvider.Messages);
        }

        string DriveToRoot(string path)
        {
            if (path.StartsWith(_defDrive))
            {
                return _defRoot + "/" + path.Substring(_defDrive.Length);
            }
            else if (path.StartsWith(_secDrive))
            {
                return _secRoot + "/" + path.Substring(_secDrive.Length);
            }
            return path;
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            // set the "existing" items before each test
            TestNavigationProvider.ExistingPaths = new List<string>()
            {
                _defRoot + "/foo/bar.txt",
                _defRoot + "/foo/baz.doc",
                _defRoot + "/foo/foo/bla.txt",
                _defRoot + "/bar.doc",
                _defRoot + "/bar/foo.txt",
                _secRoot + "/foo/blub.doc"
            };
            TestNavigationProvider.Messages.Clear();
        }

        [TestCase(_defDrive, "leaf", false)]
        [TestCase(_defDrive, "any", true)]
        [TestCase(_defDrive + "notExisting", "any", false)]
        [TestCase(_defDrive + "notExisting", "leaf", false)]
        [TestCase(_defDrive + "foo", "leaf", false)]
        [TestCase(_defDrive + "foo", "any", true)]
        [TestCase(_defDrive + "bar.doc", "leaf", true)]
        [TestCase(_defDrive + "bar.doc", "any", true)]
        [TestCase(_secDrive + "foo/blub.doc", "leaf", true)]
        [TestCase(_secDrive + "foo/blub.doc", "any", true)]
        public void NavigationProviderSupportsTestPathAnyLeaf(string path, string type, bool expected)
        {
            var cmd = "Test-Path " + path + " -PathType " + type;
            ExecuteAndCompareTypedResult(cmd, expected);
            Assert.That(TestNavigationProvider.Messages[0], Is.EqualTo("ItemExists " + DriveToRoot(path)));
        }

        [TestCase(_defDrive, true)]
        [TestCase(_defDrive + "notExisting", true)] // although not existing, because only IsContainer is checked
        [TestCase(_defDrive + "foo", true)]
        [TestCase(_defDrive + "bar.doc", false)]
        [TestCase(_secDrive + "foo/blub.doc", false)]
        public void NavigationProviderSupportsTestPathContainer(string path, bool expected)
        {
            var cmd = "Test-Path " + path + " -PathType container";
            ExecuteAndCompareTypedResult(cmd, expected);
            Assert.That(TestNavigationProvider.Messages[0], Is.EqualTo("IsItemContainer " + DriveToRoot(path)));
        }

        [TestCase(_defDrive, _defRoot)]
        [TestCase(_secDrive, _secRoot)]
        public void NavigationProviderSupportsGetItem(string drive, string root)
        {
            var cmd = "Get-Item " + drive + "foo";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "ItemExists " + root + "/foo",
                "GetItem " + root + "/foo"
            );
        }

        [Test]
        public void NavigationProviderSupportsGetItemWithWildcard()
        {
            var cmd = "Get-Item " + _defDrive + "foo/b*";
            ReferenceHost.Execute(cmd);
            AssertMessagesContain(
                "GetChildNames " + _defRoot + "/foo ReturnMatchingContainers",
                "GetItem " + _defRoot + "/foo/bar.txt",
                "GetItem " + _defRoot + "/foo/baz.doc"
            );
            AssertMessagesDoesntContain(
                "GetItem " + _defRoot + "/foo/foo",
                "GetItem " + _defRoot + "/foo/foo/bla.txt"
            );
        }

        [Test]
        public void NavigationProviderSupportsNewItem()
        {
            var path = _defDrive + "newItem.tmp";
            var rpath = _defRoot + "/newItem.tmp";
            var cmd = "New-Item " + path  + " -ItemType testType -Value testValue";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual("NewItem " + rpath + " testType testValue");
        }

        [Test]
        public void NavigationProviderSupportsRemoveItem()
        {
            var cmd = "Remove-Item " + _defDrive + "bar.doc";
            var rpath = _defRoot + "/bar.doc";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "ItemExists " + rpath,
                "HasChildItems " + rpath,
                "RemoveItem " + rpath + " False"
            );
        }

        [Test]
        public void NavigationProviderSupportsRemoveItemWithRecursion()
        {
            var cmd = "Remove-Item -Recurse " + _defDrive + "foo";
            var rpath = _defRoot + "/foo";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "ItemExists " + rpath,
                "HasChildItems " + rpath,
                "RemoveItem " + rpath + " True"
            );
        }

        [Test]
        public void NavigationProviderThrowsOnRemoveNodeWithoutRecursion()
        {
            var cmd = "Remove-Item " + _defDrive + "foo";
            Assert.Throws<CmdletInvocationException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace(cmd);
            });
        }

        [Test]
        public void NavigationProviderSupportsRenameItem()
        {
            var cmd = "Rename-Item " + _defDrive + "foo -NewName foobar";
            var rpath = _defRoot + "/foo";
            ReferenceHost.Execute(cmd);
            var expectedMsgs = new[] {
                "ItemExists " + rpath,
                "RenameItem " + rpath + " foobar"
            };
            // Powershell shomehow calls ItemExists twice. We won't check for this behavior
            var msgCount = TestNavigationProvider.Messages.Count;
            Assert.That(TestNavigationProvider.Messages[msgCount - 2], Is.EqualTo("ItemExists " + rpath));
            Assert.That(TestNavigationProvider.Messages[msgCount - 1], Is.EqualTo("RenameItem " + rpath + " foobar"));
        }
        /*

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
*/
    }
}


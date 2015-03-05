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
        private const string _defRoot = TestNavigationProvider.DefaultDriveRoot + "/";
        private const string _secDrive = TestNavigationProvider.SecondDrivePath;
        private const string _secRoot = TestNavigationProvider.SecondDriveRoot + "/";

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
                return _defRoot + path.Substring(_defDrive.Length);
            }
            else if (path.StartsWith(_secDrive))
            {
                return _secRoot + path.Substring(_secDrive.Length);
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
                _defRoot + "foo/bar.txt",
                _defRoot + "foo/baz.doc",
                _defRoot + "foo/foo/bla.txt",
                _defRoot + "bar.doc",
                _defRoot + "bar/foo.txt",
                _secRoot + "foo/blub.doc",
                _secRoot + "foo/bar.txt",
                _secRoot + "bar.txt"
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
                "ItemExists " + root + "foo",
                "GetItem " + root + "foo"
            );
        }

        [Test]
        public void NavigationProviderSupportsGetItemWithWildcard()
        {
            var cmd = "Get-Item " + _defDrive + "foo/b*";
            ReferenceHost.Execute(cmd);
            var getMsgs = (from m in TestNavigationProvider.Messages
                           where m.StartsWith("Get")
                           select m).ToArray();
            Assert.That(getMsgs, Is.EquivalentTo(new []{
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers",
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers",
                "GetItem " + _defRoot + "foo/bar.txt",
                "GetItem " + _defRoot + "foo/baz.doc"
            }));
        }

        [Test]
        public void NavigationProviderSupportsNewItem()
        {
            var path = _defDrive + "newItem.tmp";
            var rpath = _defRoot + "newItem.tmp";
            var cmd = "New-Item " + path  + " -ItemType testType -Value testValue";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual("NewItem " + rpath + " testType testValue");
        }

        [Test]
        public void NavigationProviderSupportsRemoveItem()
        {
            var cmd = "Remove-Item " + _defDrive + "bar.doc";
            var rpath = _defRoot + "bar.doc";
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
            var rpath = _defRoot + "foo";
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
            var rpath = _defRoot + "foo";
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

        [Test]
        public void NavigationProviderSupportsGetChildItemWithRecursion()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo -Recurse";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "IsItemContainer " + _defRoot + "foo",
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _defRoot + "foo",
                "GetChildItems " + _defRoot + "foo True"
            );
        }

        [Test]
        public void NavigationProviderSupportsGetChildItem()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _defRoot + "foo",
                "GetChildItems " + _defRoot + "foo False"
            );
        }

        [TestCase("-Include '*.txt'")]
        [TestCase("-Include '*.*' -Exclude '*.doc'")]
        public void NavigationProviderSupportsGetChildItemWithFilter(string filter)
        {
            var cmd = "Get-ChildItem -Recurse " + _defDrive + " " + filter;
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "ItemExists " + _defRoot,
                "IsItemContainer " + _defRoot,
                "GetChildNames " + _defRoot + " ReturnAllContainers",
                "IsItemContainer " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo ReturnAllContainers",
                "GetItem " + _defRoot + "foo/bar.txt",
                "IsItemContainer " + _defRoot + "foo/bar.txt",
                "IsItemContainer " + _defRoot + "foo/baz.doc",
                "IsItemContainer " + _defRoot + "foo/foo",
                "GetChildNames " + _defRoot + "foo/foo ReturnAllContainers",
                "GetItem " + _defRoot + "foo/foo/bla.txt",
                "IsItemContainer " + _defRoot + "foo/foo/bla.txt",
                "IsItemContainer " + _defRoot + "bar.doc",
                "IsItemContainer " + _defRoot + "bar",
                "GetChildNames " + _defRoot + "bar ReturnAllContainers",
                "GetItem " + _defRoot + "bar/foo.txt",
                "IsItemContainer " + _defRoot + "bar/foo.txt"
            );
        }

        [Test]
        public void NavigationProviderSupportsGetChildItemWithFilterInPath()
        {
            var cmd = "Get-ChildItem -Recurse " + _secDrive + "*.txt";
            ReferenceHost.Execute(cmd);
            AssertMessagesAreEqual(
                "ItemExists " + _secRoot,
                "HasChildItems " + _secRoot,
                "GetChildNames " + _secRoot + " ReturnMatchingContainers",
                "IsItemContainer " + _secRoot + "bar.txt",
                "ItemExists " + _secRoot,
                "IsItemContainer " + _secRoot,
                "GetChildNames " + _secRoot + " ReturnAllContainers",
                "IsItemContainer " + _secRoot + "foo",
                "GetChildNames " + _secRoot + "foo ReturnAllContainers",
                "IsItemContainer " + _secRoot + "foo/blub.doc",
                "GetItem " + _secRoot + "foo/bar.txt",
                "IsItemContainer " + _secRoot + "foo/bar.txt",
                "GetItem " + _secRoot + "bar.txt",
                "IsItemContainer " + _secRoot + "bar.txt"
            );
        }

        /*
         * copy
         * getchildnames
         * move
         * resolve.path
         */
    }
}


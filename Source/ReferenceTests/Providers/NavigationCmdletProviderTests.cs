using System;
using System.Linq;
using NUnit.Framework;
using TestPSSnapIn;
using System.Management.Automation;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace ReferenceTests.Providers
{
    public class NavigationCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        private const string _defDrive = TestNavigationProvider.DefaultDrivePath;
        private const string _defRoot = TestNavigationProvider.DefaultDriveRoot + "/";
        private const string _secDrive = TestNavigationProvider.SecondDrivePath;
        private const string _secRoot = TestNavigationProvider.SecondDriveRoot + "/";

        private List<string> ExecutionMessages { get { return TestNavigationProvider.Messages; } }

        EqualConstraint AreMatchedBy(params string[] expected)
        {
            // constraint like EqualTo, but allowing messages to be prepended by "? " to be optional
            var msgs = TestNavigationProvider.Messages;
            var optional = (from m in expected where m.StartsWith("? ") select m).Count();
            if (msgs.Count == expected.Length - optional)
            {
                var nonOpts = (from m in expected where !m.StartsWith("? ") select m);
                return Is.EqualTo(nonOpts);
            }
            else
            {
                var allExp = from m in expected select (m.StartsWith("? ") ? m.Substring(2) : m);
                return Is.EqualTo(allExp);
            }
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
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + root + "foo",
                "GetItem " + root + "foo"
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetItemWithWildcard()
        {
            var cmd = "Get-Item " + _defDrive + "foo/b*";
            ReferenceHost.Execute(cmd);
            // with PS "GetChildNames " + _defRoot + "foo ReturnMatchingContainers" is called
            // twice at the beginning. We won't check for this behavior
            Assert.That(ExecutionMessages, AreMatchedBy(
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers",
                "? GetChildNames " + _defRoot + "foo ReturnMatchingContainers", // optional
                "GetItem " + _defRoot + "foo/bar.txt",
                "GetItem " + _defRoot + "foo/baz.doc"
            ));
        }

        [Test]
        public void NavigationProviderSupportsNewItem()
        {
            var path = _defDrive + "newItem.tmp";
            var rpath = _defRoot + "newItem.tmp";
            var cmd = "New-Item " + path  + " -ItemType testType -Value testValue";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy("NewItem " + rpath + " testType testValue"));
        }

        [Test]
        public void NavigationProviderSupportsRemoveItem()
        {
            var cmd = "Remove-Item " + _defDrive + "bar.doc";
            var rpath = _defRoot + "bar.doc";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + rpath,
                "HasChildItems " + rpath,
                "RemoveItem " + rpath + " False"
            ));
        }

        [Test]
        public void NavigationProviderSupportsRemoveItemWithRecursion()
        {
            var cmd = "Remove-Item -Recurse " + _defDrive + "foo";
            var rpath = _defRoot + "foo";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + rpath,
                "? HasChildItems " + rpath, // optional
                "RemoveItem " + rpath + " True"
            ));
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
            // Powershell shomehow calls ItemExists twice. We won't check for this behavior
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + rpath,
                "? ItemExists " + rpath, // optional
                "RenameItem " + rpath + " foobar"
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildItemWithRecursion()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo -Recurse";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "IsItemContainer " + _defRoot + "foo",
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _defRoot + "foo",
                "GetChildItems " + _defRoot + "foo True"
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildItem()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _defRoot + "foo",
                "GetChildItems " + _defRoot + "foo False"
            ));
        }


        [Test]
        public void NavigationProviderSupportsGetChildItemFromLeaf()
        {
            var cmd = "Get-ChildItem " + _defDrive + "bar.doc";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "bar.doc",
                "IsItemContainer " + _defRoot + "bar.doc",
                "GetItem " + _defRoot + "bar.doc"
            ));
        }

        [TestCase("-Include '*.txt'")]
        [TestCase("-Include '*.*' -Exclude '*.doc'")]
        public void NavigationProviderSupportsGetChildItemWithFilter(string filter)
        {
            var cmd = "Get-ChildItem -Recurse " + _defDrive + " " + filter;
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
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
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildItemWithFilterInPath()
        {
            var cmd = "Get-ChildItem -Recurse " + _secDrive + "*.txt";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
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
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildNames()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo -Name";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers"
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildNamesFromLeaf()
        {
            var cmd = "Get-ChildItem " + _defDrive + "bar.doc -Name";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "bar.doc",
                "GetChildNames " + _defRoot + "bar.doc ReturnMatchingContainers"
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildNamesWithRecursion()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo -Name -Recurse";
            ExecuteAndCompareTypedResult(cmd, "bar.txt", "baz.doc", "foo", "foo/bla.txt");
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo/ ReturnMatchingContainers",
                "GetChildNames " + _defRoot + "foo/ ReturnAllContainers",
                "IsItemContainer " + _defRoot + "foo/bar.txt",
                "IsItemContainer " + _defRoot + "foo/baz.doc",
                "IsItemContainer " + _defRoot + "foo/foo",
                "GetChildNames " + _defRoot + "foo/foo ReturnMatchingContainers",
                "GetChildNames " + _defRoot + "foo/foo ReturnAllContainers",
                "IsItemContainer " + _defRoot + "foo/foo/bla.txt"
            ));
        }

        [TestCase("-Include *.txt")]
        [TestCase("-Include *.* -Exclude *.doc")]
        public void NavigationProviderSupportsGetChildNamesWithRecursionAndFilter(string filter)
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo -Name -Recurse " + filter;
            ExecuteAndCompareTypedResult(cmd, "bar.txt", "foo/bla.txt");
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo/ ReturnMatchingContainers",
                "GetChildNames " + _defRoot + "foo/ ReturnAllContainers",
                "IsItemContainer " + _defRoot + "foo/bar.txt",
                "IsItemContainer " + _defRoot + "foo/baz.doc",
                "IsItemContainer " + _defRoot + "foo/foo",
                "GetChildNames " + _defRoot + "foo/foo ReturnMatchingContainers",
                "GetChildNames " + _defRoot + "foo/foo ReturnAllContainers",
                "IsItemContainer " + _defRoot + "foo/foo/bla.txt"
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetChildNamesWithFilterInPathDoesntWork()
        {
            var cmd = "Get-ChildItem " + _defDrive + "foo/*.txt -Name -Recurse";
            ExecuteAndCompareTypedResult(cmd, "bar.txt");
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "HasChildItems " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers",
                "ItemExists " + _defRoot + "foo",
                "HasChildItems " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers",
                "IsItemContainer " + _defRoot + "foo/bar.txt"
            ));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NavigationProviderSupportsCopyItem(bool recurse)
        {
            var recurseParam = recurse ? " -Recurse" : "";
            var cmd = "Copy-Item " + _defDrive + "foo/ " + _secDrive + recurseParam;
            ReferenceHost.Execute(cmd);
            // in Pash the first two operations are called in reverse order (because of our code design)
            // so we need one of the two outputs
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _secRoot,
                "IsItemContainer " + _defRoot + "foo",
                "CopyItem " + _defRoot + "foo " + _secRoot + " " + recurse
            ).Or.Matches(AreMatchedBy(
                "IsItemContainer " + _secRoot,
                "ItemExists " + _defRoot + "foo",
                "IsItemContainer " + _defRoot + "foo",
                "CopyItem " + _defRoot + "foo " + _secRoot + " " + recurse
            )));
        }

        [Test]
        public void NavigationProviderThrowsOnCopyItemToOtherProvider()
        {
            var cmd = "Copy-Item " + _defDrive + "bar.doc variable:\\";
            var e = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(cmd);
            });
            Assert.That(e.Errors.Length, Is.EqualTo(1));
            Assert.That(e.Errors[0].Exception, Is.TypeOf(typeof(PSArgumentException)));
        }

        [Test]
        public void NavigationProviderThrowsOnCopyItemContainerOnLeaf()
        {
            var cmd = "Copy-Item -Recurse " + _defDrive + "foo/ " + _secDrive + "bar.txt";
            var e = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(cmd);
            });
            Assert.That(e.Errors.Length, Is.EqualTo(1));
            Assert.That(e.Errors[0].Exception, Is.TypeOf(typeof(PSArgumentException)));
        }

        [Test, Ignore("This somehow doesn't work as Powershell mixes up provider with the last parameter")]
        public void NavigationProviderSupportsCopyItemWithoutContainers()
        {
            var cmd = "Copy-Item " + _defDrive + "foo/ " + _secDrive + " -Recurse -Container:$false";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                // Mesages?
            ));
        }

        [Test]
        public void NavigationProviderThrowsOnMoveItemToOtherProvider()
        {
            var cmd = "Move-Item " + _defDrive + "bar.doc variable:\\";
            var e = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(cmd);
            });
            Assert.That(e.Errors.Length, Is.EqualTo(1));
            Assert.That(e.Errors[0].Exception, Is.TypeOf(typeof(PSArgumentException)));
        }

        [Test]
        public void NavigationProviderSupportsMoveItem()
        {
            var cmd = "Move-Item " + _defDrive + "foo/ " + _secDrive;
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                // well we have here a triple-check. Maybe remove that for check against Pash
                "ItemExists " + _defRoot + "foo",
                "ItemExists " + _defRoot + "foo",
                "ItemExists " + _defRoot + "foo",
                "MoveItem " + _defRoot + "foo " + _secRoot
            ));
        }

        [Test]
        public void NavigationProviderSupportsMoveItemContainerOnLeaf()
        {
            var cmd = "Move-Item " + _defDrive + "foo/ " + _secDrive + "bar.txt";
            ReferenceHost.Execute(cmd);
            Assert.That(ExecutionMessages, AreMatchedBy(
                // well we have here a triple-check. Maybe remove that for check against Pash
                "ItemExists " + _defRoot + "foo",
                "ItemExists " + _defRoot + "foo",
                "ItemExists " + _defRoot + "foo",
                "MoveItem " + _defRoot + "foo " + _secRoot + "bar.txt"
            ));
        }

        [Test]
        public void NavigationProviderSupportsResolvePath()
        {
            var cmd = "(Resolve-Path " + _defDrive + "foo/*.txt).Path";
            ExecuteAndCompareTypedResult(cmd, TestNavigationProvider.DefaultDriveName + ":\\foo/bar.txt");
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "HasChildItems " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers"
            ));
        }

        [Test]
        public void NavigationProviderSupportsResolvePathRelative()
        {
            var cmd = NewlineJoin(
                "Set-Location " + _defDrive,
                "Resolve-Path " + _defDrive + "foo/*.txt -Relative"
            );
            var rootWithoutSlash = _defRoot.Substring(0, _defRoot.Length -1);
            ExecuteAndCompareTypedResult(cmd, "./foo/bar.txt");
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot,
                "NormalizeRelativePath " + _defRoot + " " + rootWithoutSlash,
                "IsItemContainer " + _defRoot,
                "ItemExists " + _defRoot + "foo",
                "HasChildItems " + _defRoot + "foo",
                "GetChildNames " + _defRoot + "foo ReturnMatchingContainers",
                "NormalizeRelativePath " + _defRoot + "foo/bar.txt " + _defRoot
            ));
        }

        [Test]
        public void NavigationProviderSupportsGetItemWithRelativePath()
        {
            var cmd = NewlineJoin(
                "Set-Location " + _defDrive + "foo",
                "Get-Item ../bar.doc"
            );
            ReferenceHost.Execute(cmd);
            var rootWithoutSlash = _defRoot.Substring(0, _defRoot.Length - 1);
            Assert.That(ExecutionMessages, AreMatchedBy(
                "ItemExists " + _defRoot + "foo",
                "NormalizeRelativePath " + _defRoot + "foo " + rootWithoutSlash,
                "IsItemContainer " + _defRoot + "foo",
                "ItemExists " + _defRoot + "bar.doc",
                "GetItem " + _defRoot + "bar.doc"
            ));
        }

    }
}


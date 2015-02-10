using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
            var cmd = "Get-Item " + TestContainerProvider.DefaultItemPath;
            ExecuteAndCompareTypedResult(cmd, TestContainerProvider.DefaultItemValue);
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
        public void ContainerProviderSupportsTestPath(string path, string type, bool expected)
        {
            var cmd = "Test-Path " + path + " -PathType " + type;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [TestCase(TestContainerProvider.DefaultDrivePath + "newLeaf1", "", "testValue", "testValue")]
        [TestCase(TestContainerProvider.DefaultDrivePath + "newLeaf2", "uppercase", "lower", "LOWER")]
        public void ContainerProviderSupportsNewItem(string path, string type, string value, string expected)
        {
            var cmd = NewlineJoin(
                "$ni = New-Item " + path + " -ItemType " + type + " -Value '" + value + "'",
                "$gi = Get-Item " + path,
                "[object]::ReferenceEquals($ni, $gi)",
                "$gi"
            );
            ExecuteAndCompareTypedResult(cmd, true, expected);
        }

        [Test]
        public void ContainerProviderSupportsNewItemByName()
        {
            var cmd = NewlineJoin(
                "$ni = New-Item " + TestContainerProvider.DefaultDrivePath + "foo/bar/baz -Name new -Value 'leafValue'",
                "$gi = Get-Item " + TestContainerProvider.DefaultDrivePath + "new", // important: ContainerProvider ignores the path if name is provided
                "[object]::ReferenceEquals($ni, $gi)",
                "$gi"
            );
            ExecuteAndCompareTypedResult(cmd, true, "leafValue");
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
        public void ContainerProviderSupportsRemoveItem()
        {
            var path = TestContainerProvider.DefaultItemPath;
            var cmd = NewlineJoin(
                "Test-Path " + path,
                "Remove-Item " + path,
                "Test-Path " + path
            );
            ExecuteAndCompareTypedResult(cmd, true, false);
        }

        [Test]
        public void ContainerProviderSupportsRemoveItemWithRecursion()
        {
            var childPath = TestContainerProvider.DefaultDrivePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + childPath + " -Value 'testValue'",
                "Test-Path " + childPath,
                "Test-Path " + TestContainerProvider.DefaultItemPath,
                "Remove-Item " + TestContainerProvider.DefaultDrivePath + " -Recurse",
                "Test-Path " + childPath,
                "Test-Path " + TestContainerProvider.DefaultItemPath
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
            var cmd = "Remove-Item " +  TestContainerProvider.DefaultDrivePath;
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
        public void ContainerProviderThrowsOnRenameItemWithWithExistingDestination()
        {
            var newPath = TestContainerProvider.DefaultDrivePath + "copiedItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -ItemType -Value 'testValue'",
                "Rename-Item " + TestContainerProvider.DefaultItemPath + " -NewName copiedItem"
            );
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                ReferenceHost.Execute(cmd);
            });
        }

        [Test]
        public void ContainerProviderSupportsCopyItem()
        {
            var origPath = TestContainerProvider.DefaultItemPath;
            var copyPath = TestContainerProvider.DefaultDrivePath + "copiedItem";
            var cmd = NewlineJoin(
                "Test-Path " + origPath,
                "Test-Path " + copyPath,
                "$ci = Copy-Item " + origPath + " " + copyPath + " -PassThru",
                "Test-Path " + origPath,
                "Test-Path " + copyPath,
                "$gi = Get-Item " + copyPath,
                "$ci",
                "$gi"
            );
            var val = TestContainerProvider.DefaultItemValue;
            ExecuteAndCompareTypedResult(cmd, true, false, true, true, val, val);
        }

        [Test]
        public void ContainerProviderSupportsCopyItemToExistingDestination()
        {
            var itemPath = TestContainerProvider.DefaultItemPath;
            var newPath = TestContainerProvider.DefaultDrivePath + "copiedItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -ItemType -Value 'testValue'",
                "Copy-Item " + newPath + " " + itemPath,
                "Get-Item " + itemPath
            );
            ExecuteAndCompareTypedResult(cmd, "testValue");
        }

        [Test]
        public void ContainerProviderSupportsGetChildItem()
        {
            var newPath = TestContainerProvider.DefaultDrivePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -Value 'someValue'",
                "Get-ChildItem " + TestContainerProvider.DefaultDrivePath
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item("someValue"));
            Assert.That(results, Contains.Item(TestContainerProvider.DefaultItemValue));
        }

        // multiple ways to only get test1 and test3, but not test2 or the default item
        [TestCase(" -Include 'test*' -Exclude '*2'")]
        [TestCase(" -Exclude '*2','" + TestContainerProvider.DefaultItemName + "'")]
        [TestCase(" -Filter '(test1|test3)'")] // custom filter understands regex
        [TestCase(" -Filter 'test\\d' -Exclude 'test2'")] // custom filter with exclude mixed
        [TestCase("*[123] -Exclude 'test2'")] // as part of the path
        public void ContainerProviderSupportsGetChildItemWithFilters(string args)
        {
            var pathPrefix = TestContainerProvider.DefaultDrivePath;
            var cmd = NewlineJoin(
                "$ni = New-Item " + pathPrefix + "test1 -Value 't1'",
                "$ni = New-Item " + pathPrefix + "test2 -Value 't2'",
                "$ni = New-Item " + pathPrefix + "test3 -Value 't3'",
                "Get-ChildItem " + TestContainerProvider.DefaultDrivePath + args
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item("t1"));
            Assert.That(results, Contains.Item("t3"));
        }

        [Test]
        public void ContainerProviderSupportsGetChildItemNames()
        {
            var newPath = TestContainerProvider.DefaultDrivePath + "someItem";
            var cmd = NewlineJoin(
                "$ni = New-Item " + newPath + " -Value 'someValue'",
                "Get-ChildItem -Name " + TestContainerProvider.DefaultDrivePath
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item("someItem"));
            Assert.That(results, Contains.Item(TestContainerProvider.DefaultItemName));
        }

        // multiple ways to only get test1 and test3, but not test2 or the default item
        [TestCase(" -Include 'test*' -Exclude '*2'")]
        [TestCase(" -Exclude '*2','" + TestContainerProvider.DefaultItemName + "'")]
        [TestCase(" -Filter '(test1|test3)'")] // custom filter understands regex
        [TestCase(" -Filter 'test\\d' -Exclude 'test2'")] // custom filter with exclude mixed
        [TestCase("*[123] -Exclude 'test2'")] // as part of the path
        public void ContainerProviderSupportsGetChildItemNameWithFilters(string args)
        {
            var pathPrefix = TestContainerProvider.DefaultDrivePath;
            var cmd = NewlineJoin(
                "$ni = New-Item " + pathPrefix + "test1 -Value 't1'",
                "$ni = New-Item " + pathPrefix + "test2 -Value 't2'",
                "$ni = New-Item " + pathPrefix + "test3 -Value 't3'",
                "Get-ChildItem -Name " + TestContainerProvider.DefaultDrivePath + args
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item("test1"));
            Assert.That(results, Contains.Item("test3"));
        }

        [TestCase("* -Include 'test*' -Exclude '*2'")]
        [TestCase("* -Exclude '*2','" + TestContainerProvider.DefaultItemName + "'")]
        [TestCase("* -Filter '(test1|test3)'")] // custom filter understands regex
        [TestCase("* -Filter 'test\\d' -Exclude 'test2'")] // custom filter with exclude mixed
        [TestCase("*[123] -Exclude 'test2'")] // path gets globbed
        public void ContainerProviderSupportsGetItemWithGlobbingAndFilters(string args)
        {
            var pathPrefix = TestContainerProvider.DefaultDrivePath;
            var cmd = NewlineJoin(
                "$ni = New-Item " + pathPrefix + "test1 -Value 't1'",
                "$ni = New-Item " + pathPrefix + "test2 -Value 't2'",
                "$ni = New-Item " + pathPrefix + "test3 -Value 't3'",
                "Get-Item " + TestContainerProvider.DefaultDrivePath + args
            );
            var psObjResults = ReferenceHost.RawExecute(cmd);
            var results = (from r in psObjResults select r.BaseObject).ToList();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Contains.Item("t1"));
            Assert.That(results, Contains.Item("t3"));
        }

        [Test]
        public void ContainerProviderSupportsGetItemWithLiteralPath()
        {
            var pathPrefix = TestContainerProvider.DefaultDrivePath;
            var cmd = NewlineJoin(
                "$ni = New-Item " + pathPrefix + "test1 -Value 't1'",
                "$ni = New-Item '" + pathPrefix + "*' -Value 'asterisk'", // '*' is part of the name
                // should avoid globbing and just return the one item instead of all
                "Get-Item -LiteralPath " + TestContainerProvider.DefaultDrivePath + "*"
            );
            ExecuteAndCompareTypedResult(cmd, "asterisk");
        }

        [Test]
        public void ContainerProviderSupportsSetGetLocation()
        {
            // container providers usually don't support hierarchy, so we cannt set-location to a node
            var cmd = NewlineJoin(
                "Set-Location " + TestContainerProvider.DefaultDriveRoot,
                "(Get-Location).Path",
                "Get-Item " + TestContainerProvider.DefaultItemName // make sure we interact in the new location
            );
            var adjustedDrivePath = AdjustSlashes(TestContainerProvider.DefaultDriveRoot);
            ExecuteAndCompareTypedResult(cmd, adjustedDrivePath, TestContainerProvider.DefaultItemValue);
        }

        [Test]
        public void ContainerProviderSupportsPushPopLocation()
        {
            var cmd = NewlineJoin(
                "$origLoc = (Get-Location).Path",
                "Push-Location " + TestContainerProvider.DefaultDriveRoot,
                "(Get-Location).Path",
                "Pop-Location",
                "$origLoc -eq (Get-Location).Path"
            );
            var adjustedDrivePath = AdjustSlashes(TestContainerProvider.DefaultDrivePath);
            ExecuteAndCompareTypedResult(cmd, adjustedDrivePath, true);
        }

        private static string AdjustSlashes(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}

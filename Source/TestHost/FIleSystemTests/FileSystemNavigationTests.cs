using System;
using NUnit.Framework;
using System.Management;

namespace TestHost.FileSystemTests
{
    [TestFixture]
    public class FileSystemNavigationTests : FileSystemTestBase
    {
        [Test, Explicit("Currently failing on Travis-ci, not on windows or mac locally though?")]
        public void CanSetLocationIntoSubDirectory()
        {
            var rootPath = base.SetupFileSystemWithStructure(new[]{
                "/FolderA/SubFolderA/FileA".NormalizeSlashes()
            });

            var result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + rootPath,
                "Get-Location");

            result.Trim().ShouldEqual((string)rootPath);

            result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + (rootPath + "/FolderA").NormalizeSlashes(),
                "Get-Location");


            result.Trim().ShouldEqual(((string)rootPath + "/FolderA").NormalizeSlashes());

            result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + (rootPath + "/FolderA/SubfolderA").NormalizeSlashes(),
                "Get-Location");

            result.Trim().PathShouldEqual(((string)rootPath + "/FolderA/SubfolderA").NormalizeSlashes());

        }

        [Test]
        [TestCase(".", "/a/b/c/d/e/f/g", "should not change directories")]
        [TestCase("./../../../../../../", "/a", "should navigate up a bunch of directories")]
        [TestCase("./../../../../../../../", "", "should navigate up a bunch of directories")]
        [TestCase("./..", "/a/b/c/d/e/f", "should nav up one dir")]
        [TestCase("./../", "/a/b/c/d/e/f", "should nav up one dir")]
        [TestCase("..", "/a/b/c/d/e/f", "should nav up one dir")]
        [TestCase("../", "/a/b/c/d/e/f", "should nav up one dir")]
        [TestCase("../g/h", "/a/b/c/d/e/f/g/h", "should nav up one dir and then back down 2 dirs")]
        [TestCase("../g/h/../../g/h", "/a/b/c/d/e/f/g/h", "should nav up one dir and then back down 2 dirs")]
        [TestCase("h", "/a/b/c/d/e/f/g/h", "should down one dir")]
        public void CDWithTwoPeriodsShouldMoveUpOneDirectory(string setLocationParam, string expectedLocation, string errorMessage)
        {
            var rootPath = base.SetupFileSystemWithStructure(new[]{
                "/a/b/c/d/e/f/g/h/i/j/k/l/m/n/o/p"
            });

            var result = TestHost.ExecuteWithZeroErrors("Set-Location " + rootPath + "/a/b/c/d/e/f/g",
                                                        "Set-Location " + setLocationParam,
                                                        "Get-Location");

            result.Trim().PathShouldEqual(rootPath + expectedLocation);
        }

        [Test]
        public void CDToInvalidDirectoryShouldThrowError()
        {
            var currentLocation = "Get-Location".Exec();

            var result = TestHost.ExecuteWithZeroErrors("set-location thisFolderReallyShouldNotExistNoReallyItShouldNotBeAnywhereOnAnyDiskAnywherePERIOD");
            result.ShouldContain("Cannot find path");
            result.ShouldContain("because it does not exist.");

            var currentLocationAfterBadCD = "Get-Location".Exec();

            currentLocation.PathShouldEqual(currentLocationAfterBadCD);
        }

        [Test]
        public void CDToSlashShouldTakeYouToTheRootOfTheFileSystemDrive()
        {
            var currentLocation = "Set-Location /; Get-Location".Exec();

            //TODO: how to assert this is "C:\" on windows?
            currentLocation.PathShouldEqual("/");
        }

        [Test]
        [TestCase("/", "root should be root")]
        [TestCase("..", "one up from root should still be root")]
        [TestCase("../..", "two up from root should still be root")]
        public void CDShouldRemainAtRootLocation(string cdCommand, string errorMessage)
        {
            var rootPath = "Set-Location /;".Exec();

            var rootPathAfterCDUpOneDirFromRoot = ("Set-Location /; Set-Location " + cdCommand).Exec();

            rootPathAfterCDUpOneDirFromRoot.PathShouldEqual(rootPath, errorMessage);
        }
    }

    public static partial class _
    {
        public static string Exec(this string command)
        {
            var result = TestHost.ExecuteWithZeroErrors(command);
            if (result.EndsWith(Environment.NewLine))
            {
                // trim the new-line at the end
                return result.Substring(0, result.Length - Environment.NewLine.Length);
            }
            return result;
        }
    }
}

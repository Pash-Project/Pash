using System;
using NUnit.Framework;
using System.Management;

namespace TestHost
{
    [TestFixture]
    public class FileSystemNavigationTests : FileSystemTestBase
    {
        [Test]
        public void CanSetLocationIntoSubDirectory()
        {
            var rootPath = base.SetupFileSystemWithStructure(new []{
                "/FolderA/SubFolderA/FileA"
            });

            var result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + rootPath,
                "Get-Location");
            
            result.Trim().ShouldEqual((string)rootPath);

            result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + rootPath + "/FolderA",
                "Get-Location");


            result.Trim().ShouldEqual((string)rootPath + "/FolderA");
            
            result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + rootPath + "/FolderA/SubfolderA",
                "Get-Location");
            
            result.Trim().ShouldEqual((string)rootPath + "/FolderA/SubfolderA");
            
        }

        [Test]
        [TestCase("..", "/a/b/c/d/e/f", "")]
        public void CDWithTwoPeriodsShouldMoveUpOneDirectory(string setLocationParam, string expectedLocation, string errorMessage)
        {
            var rootPath = base.SetupFileSystemWithStructure(new []{
                "/a/b/c/d/e/f/g/h/i/j/k/l/m/n/o/p"
            });

            // set the start directory
            TestHost.ExecuteWithZeroErrors("Set-Location " + rootPath + "/a/b/c/d/e/f/g");
            
            var result = TestHost.ExecuteWithZeroErrors("Set-Location " + setLocationParam,
                                                        "Get-Location");
            
            result.Trim().ShouldEqual(rootPath + expectedLocation);
        }
    }
}


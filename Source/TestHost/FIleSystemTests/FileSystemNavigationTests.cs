using System;
using NUnit.Framework;
using System.Management;

namespace TestHost
{
    [TestFixture]
    public class FileSystemNavigationTests : FileSystemTestBase
    {
        Path _path;

        [SetUp]
        public void Setup()
        {
            _path = base.SetupFileSystemWithStructure(new []{
                "/FolderA/SubFolderA/FileA"
            });
        }

        [Test]
        public void CanSetLocationIntoSubDirectory()
        {
            // notice typo
            var result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + _path,
                "Get-Location");
            
            result.Trim().ShouldEqual((string)_path);

            result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + _path + "/FolderA",
                "Get-Location");


            result.Trim().ShouldEqual((string)_path + "/FolderA");
            
            result = TestHost.ExecuteWithZeroErrors(
                "Set-Location " + _path + "/FolderA/SubfolderA",
                "Get-Location");
            
            result.Trim().ShouldEqual((string)_path + "/FolderA/SubfolderA");
            
        }
    }
}


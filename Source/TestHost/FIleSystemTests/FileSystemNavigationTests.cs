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

        // FIXME: get-location should not be returning the /:\\ 
        [Test, Ignore]
        public void CanSetLocationIntoSubDirectory()
        {
            // notice typo
            var result = TestHost.ExecuteWithZeroErrors("set-location " + _path, "Get-Location");

            result.ShouldEqual((string)_path);

        }
    }
}


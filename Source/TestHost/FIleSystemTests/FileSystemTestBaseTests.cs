using System;
using System.Management;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    public class FileSystemTestBaseTests : FileSystemTestBase
    {
        [Test]
        public void HelperCanCreateDirectory()
        {
            Path rootPath = base.SetupFileSystemWithStructure(new[]{"/foo/"});
            string path = rootPath.Combine("/foo");
            System.IO.Directory.Exists(path).ShouldBeTrue("Directory not found: " + path);
        }

        [Test]
        public void HelperCanCreateFileInDirectory()
        {
            Path rootPath = base.SetupFileSystemWithStructure(new[]{"/foo/bar"});
            string path = rootPath.Combine("/foo/bar");
            System.IO.File.Exists(path).ShouldBeTrue("File not found: " + path);
        }

        [Test]
        public void HelperCanCreateFileInSubSubSubDirectory()
        {
            Path rootPath = base.SetupFileSystemWithStructure(new[]{
                "/foo/",
                "/foo/bar/",
                "/foo/bar/baz/",
                "/foo/bar/baz/boo",
            });
            string path = rootPath.Combine("/foo/bar/baz/boo");
            System.IO.File.Exists(path).ShouldBeTrue("File not found: " + path);
        }
    }
}


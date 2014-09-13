using System;
using NUnit.Framework;
using System.IO;

namespace ReferenceTests.Providers
{
    [TestFixture]
    public class FileSystemProviderTests : ReferenceTestBase
    {
        [Test]
        public void CanCreateDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "__tempdir");
            AddCleanupDir(dir);
            var items = ReferenceHost.Execute(NewlineJoin(
                String.Format("New-Item -path '{0}' -type directory | foreach-object {{$_.FullName}}", dir))
                                              );
            var dirinfo = new DirectoryInfo(dir);
            Assert.True(dirinfo.Exists, "Directory was not created");
            Assert.AreEqual(NewlineJoin(dirinfo.FullName), items);
        }

        [Test]
        public void CanCreateRelativeDir()
        {
            Environment.CurrentDirectory = Path.GetTempPath();
            var dir = Path.Combine(".", "__tempdir");
            AddCleanupDir(dir);
            var items = ReferenceHost.Execute(NewlineJoin(
                String.Format("New-Item -path '{0}' -type directory | foreach-object {{$_.FullName}}", dir))
            );
            var dirinfo = new DirectoryInfo(dir);
            Assert.True(dirinfo.Exists, "Directory was not created");
            Assert.AreEqual(NewlineJoin(dirinfo.FullName), items);
        }

        [Test]
        public void CanCreateRecursiveDir()
        {
            var parentDir = Path.Combine(Path.GetTempPath (), "__tempdir");
            var recDir = Path.Combine(parentDir, "recursive");
            AddCleanupDir(recDir);
            AddCleanupDir(parentDir);
            var items = ReferenceHost.Execute (NewlineJoin (
                String.Format ("New-Item -path '{0}' -type directory | foreach-object {{$_.FullName}}", recDir))
            );
            var pDirinfo = new DirectoryInfo(parentDir);
            var rDirinfo = new DirectoryInfo(recDir);
            Assert.True(pDirinfo.Exists, "Parent directory was not created");
            Assert.True(rDirinfo.Exists, "Recursive directory was not created");
            Assert.AreEqual(NewlineJoin (rDirinfo.FullName), items);
        }

        [Test]
        public void CreateDirIsResistentAgainstDots()
        {
            string sep = Path.DirectorySeparatorChar.ToString();
            var easydir = Path.Combine(Path.GetTempPath (), "__tempdir");
            var dir = Path.Combine(Path.GetTempPath(), String.Format("foo{0}..{0}.{0}__tempdir", sep));
            AddCleanupDir(easydir);
            var items = ReferenceHost.Execute(NewlineJoin(
                String.Format("New-Item -path '{0}' -type directory | foreach-object {{$_.FullName}}", dir))
            );
            var dirinfo = new DirectoryInfo(easydir);
            Assert.True(dirinfo.Exists, "Directory was not created");
            Assert.AreEqual(NewlineJoin(dirinfo.FullName), items);
        }

        [Test]
        public void CanCreateFile()
        {
            var file = Path.Combine(Path.GetTempPath(), "__tempfile");
            AddCleanupFile(file);
            var items = ReferenceHost.Execute(NewlineJoin(
                String.Format("New-Item -path '{0}' -type file | foreach-object {{$_.FullName}}", file))
                                              );
            var fileinfo = new FileInfo(file);
            Assert.True(fileinfo.Exists, "File was not created");
            Assert.AreEqual(NewlineJoin(fileinfo.FullName), items);
        }

        [Test]
        public void CanCreateFileInSubdirWithForce()
        {
            var dir = Path.Combine(Path.GetTempPath(), "__tempdir");
            var file = Path.Combine(dir, "__tempfile");
            AddCleanupFile(file);
            AddCleanupDir(dir);
            var items = ReferenceHost.Execute(NewlineJoin(
                String.Format("New-Item -path '{0}' -type file -force | foreach-object {{$_.FullName}}", file))
                                              );
            var dirinfo = new DirectoryInfo(dir);
            var fileinfo = new FileInfo(file);
            Assert.True(dirinfo.Exists, "Directory was not created");
            Assert.True(fileinfo.Exists, "File was not created");
            Assert.AreEqual(NewlineJoin(fileinfo.FullName), items);
        }

        [Test]
        public void CantCreateFileInSubdirWithoutForce()
        {
            var dir = Path.Combine(Path.GetTempPath(), "__tempdir");
            var file = Path.Combine(dir, "__tempfile");
            AddCleanupFile(file); //just in case
            AddCleanupDir(dir);
            // TODO: need to fix the exception type somewhere in the pipeline processing
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate() {
                ReferenceHost.Execute(NewlineJoin(
                    String.Format("New-Item -path '{0}' -type file | foreach-object {{$_.FullName}}", file))
                );
            });
            var dirinfo = new DirectoryInfo(dir);
            var fileinfo = new FileInfo(file);
            Assert.False(dirinfo.Exists, "Directory was created anyway");
            Assert.False(fileinfo.Exists, "File was created anyway");
        }

        [Test]
        public void CanCreateFileWithContent()
        {
            var file = Path.Combine(Path.GetTempPath(), "__tempfile");
            AddCleanupFile(file);
            ReferenceHost.Execute(NewlineJoin(
                String.Format("New-Item -path '{0}' -type file -value 'foobar'", file))
                                  );
            var fileinfo = new FileInfo(file);
            Assert.True(fileinfo.Exists, "File was not created");
            Assert.AreEqual("foobar", String.Join("\n", ReadLinesFromFile(file)));
        }

        [Test]
        public void CantOverwriteFileWithoutForce()
        {
            var file = Path.Combine(Path.GetTempPath(), "__tempfile");
            AddCleanupFile(file);
            var cmd = NewlineJoin(String.Format("New-Item -path '{0}' -type file", file));
            ReferenceHost.Execute(cmd);
            // TODO: need to fix the exception type somewhere in the pipeline processing
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate() {
                ReferenceHost.Execute(cmd);
            });
        }

        [Test]
        public void CanOverwriteFileWithForce()
        {
            var file = Path.Combine(Path.GetTempPath(), "__tempfile");
            AddCleanupFile(file);
            var cmd1 = NewlineJoin(String.Format("New-Item -path '{0}' -type file -value 'a'", file));
            var cmd2 = NewlineJoin(String.Format("New-Item -force -path '{0}' -type file -value 'b'", file));
            ReferenceHost.Execute(cmd1);
            Assert.DoesNotThrow(delegate() {
                ReferenceHost.Execute(cmd2);
            });
            Assert.AreEqual("b", String.Join("\n", ReadLinesFromFile(file)));
        }
    }
}


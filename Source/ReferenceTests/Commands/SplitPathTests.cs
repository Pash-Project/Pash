// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.IO;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class SplitPathTests : ReferenceTestBase
    {
        [Test]
        public void OneParentFolder()
        {
            string result = ReferenceHost.Execute(string.Format("Split-Path 'parent{0}child'", Path.DirectorySeparatorChar));

            Assert.AreEqual("parent" + Environment.NewLine, result);
        }

        [Test]
        public void OneParentFolderOutputIsStringType()
        {
            string result = ReferenceHost.Execute(string.Format("(Split-Path 'parent{0}child').GetType().Name", Path.DirectorySeparatorChar));

            Assert.AreEqual("String" + Environment.NewLine, result);
        }

        [Test]
        public void TwoParentFolders()
        {
            string result = ReferenceHost.Execute(string.Format("Split-Path parent1{0}child,parent2{0}child", Path.DirectorySeparatorChar));

            Assert.AreEqual(string.Format("parent1{0}parent2{0}", Environment.NewLine), result);
        }

        [Test]
        public void PathFromPipelineHasOneParentFolder()
        {
            string result = ReferenceHost.Execute(string.Format("'parent{0}child' | Split-Path", Path.DirectorySeparatorChar));

            Assert.AreEqual("parent" + Environment.NewLine, result);
        }

        [Test]
        public void FullFilePathIncludingDrive()
        {
            string fullPath = CreateFile(String.Empty, ".txt");
            string directory = Path.GetDirectoryName(fullPath);
            string result = ReferenceHost.Execute(string.Format("Split-Path '{0}'", fullPath));

            Assert.AreEqual(directory + Environment.NewLine, result);
        }

        [Test]
        public void Leaf()
        {
            string command = string.Format("Split-Path -Leaf parent{0}child.txt", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("child.txt" + Environment.NewLine, result);
        }

        [Test]
        public void LeafOutputIsStringType()
        {
            string command = string.Format("(Split-Path -Leaf parent{0}child.txt).GetType().Name", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("String" + Environment.NewLine, result);
        }

        [Test]
        public void LeafTwoItems()
        {
            string command = string.Format("Split-Path -Leaf parent1{0}child1.txt,parent2{0}child2.txt", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(NewlineJoin("child1.txt", "child2.txt"), result);
        }

        [Test]
        public void LeafWithNoChild()
        {
            string result = ReferenceHost.Execute("Split-Path -Leaf parent");

            Assert.AreEqual("parent" + Environment.NewLine, result);
        }

        [Test]
        public void LeafOfEnvironmentDriveWithOneChild()
        {
            string result = ReferenceHost.Execute("Split-Path -Leaf env:foo");

            Assert.AreEqual("foo" + Environment.NewLine, result);
        }

        [Test]
        public void LeafOfEnvironmentDriveWithNoChild()
        {
            string result = ReferenceHost.Execute("Split-Path -Leaf env:");

            Assert.AreEqual(Environment.NewLine, result);
        }

        [Test]
        public void LeafOfEnvironmentDriveWithDirectorySeparatorAndOneChild()
        {
            string command = string.Format("Split-Path -Leaf env:{0}foo", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("foo" + Environment.NewLine, result);
        }

        [Test]
        public void LeafForFullFilePathIncludingDrive()
        {
            string fullPath = CreateFile(String.Empty, ".txt");
            string fileName = Path.GetFileName(fullPath);
            string result = ReferenceHost.Execute(string.Format("Split-Path -Leaf '{0}'", fullPath));

            Assert.AreEqual(fileName + Environment.NewLine, result);
        }

        [Test]
        public void IsAbsoluteIsFalseForNonAbsoluteFilePath()
        {
            string result = ReferenceHost.Execute("Split-Path -IsAbsolute abc");

            Assert.AreEqual("False" + Environment.NewLine, result);
        }

        [Test]
        public void IsAbsoluteIsTrueForFullPath()
        {
            string fullPath = CreateFile(String.Empty, ".txt");
            string result = ReferenceHost.Execute(string.Format("Split-Path -IsAbsolute -Path '{0}'", fullPath));

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void BackslashIsNotAbsoluteOnPlatformsWithCorrectSeparatorAsBackslash()
        {
            string result = ReferenceHost.Execute(@"Split-Path -IsAbsolute \ ");

            bool expected = (Path.DirectorySeparatorChar != '\\');
            Assert.AreEqual(expected.ToString() + Environment.NewLine, result);
        }

        [Test]
        public void NoQualifierForEnvironmentDriveWithOneChild()
        {
            string result = ReferenceHost.Execute("Split-Path -NoQualifier env:foo");

            Assert.AreEqual("foo" + Environment.NewLine, result);
        }

        [Test]
        public void NoQualifierForEnvironmentDriveWithNoChild()
        {
            string result = ReferenceHost.Execute("Split-Path -NoQualifier env:");

            Assert.AreEqual(Environment.NewLine, result);
        }

        [Test]
        public void NoQualifierForEnvironmentDriveWithDirectorySeparatorAndOneChild()
        {
            string command = string.Format("Split-Path -NoQualifier env:{0}foo", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(Path.DirectorySeparatorChar + "foo" + Environment.NewLine, result);
        }

        [Test]
        public void NoQualifierForDriveWithTwoSubDirectories()
        {
            string command = string.Format("Split-Path -NoQualifier C:{0}foo{0}bar", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(String.Format("{0}foo{0}bar", Path.DirectorySeparatorChar) + Environment.NewLine, result);
        }

        [Test]
        public void QualifierForEnvironmentDriveWithOneChild()
        {
            string result = ReferenceHost.Execute("Split-Path -Qualifier env:foo");

            Assert.AreEqual("env:" + Environment.NewLine, result);
        }

        [Test]
        public void QualifierForEnvironmentDriveWithNoChild()
        {
            string result = ReferenceHost.Execute("Split-Path -Qualifier env:");

            Assert.AreEqual("env:" + Environment.NewLine, result);
        }

        [Test]
        public void QualifierForEnvironmentDriveWithDirectorySeparatorAndOneChild()
        {
            string command = string.Format("Split-Path -Qualifier env:{0}foo", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("env:" + Environment.NewLine, result);
        }

        [Test]
        public void QualifierForDriveWithTwoSubDirectories()
        {
            string command = string.Format("Split-Path -Qualifier C:{0}foo{0}bar", Path.DirectorySeparatorChar);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("C:" + Environment.NewLine, result);
        }

        [Test]
        public void QualifierForPathWithNoDrive()
        {
            string command = string.Format("Split-Path -Qualifier foo{0}bar", Path.DirectorySeparatorChar);
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate
            {
                ReferenceHost.Execute(command);
            });
        }

        [Test]
        public void ResolveUnknownFile()
        {
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate
            {
                ReferenceHost.Execute("Split-Path -Resolve UnknownFileToResolve.txt");
            });
        }

        [Test]
        public void ResolveLeafForTwoFilesUsingWildcard()
        {
            string fileName1 = CreateFile(String.Empty, ".test");
            string leaf1 = Path.GetFileName(fileName1);
            string fileName2 = CreateFile(String.Empty, ".test");
            string leaf2 = Path.GetFileName(fileName2);
            string directory = Path.GetDirectoryName(fileName1);

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", directory),
                "Split-Path -Resolve -Leaf *.test"
            });

            Assert.AreEqual(NewlineJoin(leaf1, leaf2), result);
        }

        [Test]
        public void ResolveLeafForTwoPathsUsingWildcard()
        {
            string fileName1 = CreateFile(String.Empty, ".test1");
            string leaf1 = Path.GetFileName(fileName1);
            string fileName2 = CreateFile(String.Empty, ".test2");
            string leaf2 = Path.GetFileName(fileName2);
            string directory = Path.GetDirectoryName(fileName1);

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", directory),
                "Split-Path -Resolve -Leaf *.test1,*.test2"
            });

            Assert.AreEqual(NewlineJoin(leaf1, leaf2), result);
        }

        [Test]
        public void ResolveParentForTwoFilesUsingWildcard()
        {
            string fileName1 = CreateFile(String.Empty, ".test");
            Path.GetFileName(fileName1);
            string fileName2 = CreateFile(String.Empty, ".test");
            Path.GetFileName(fileName2);
            string directory = Path.GetDirectoryName(fileName1);
            directory = GetDirectoryFullPath(directory);

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", directory),
                "Split-Path -Resolve *.test"
            });

            Assert.AreEqual(NewlineJoin(directory, directory), result);
        }

        [Test]
        public void ResolveLeafForTwoDirectoriesUsingWildcard()
        {
            string tempPath = Path.GetTempPath();
            string directory1 = Path.Combine(tempPath, "TestDirectory1");
            Directory.CreateDirectory(directory1);
            string directory2 = Path.Combine(tempPath, "TestDirectory2");
            Directory.CreateDirectory(directory2);
            AddCleanupDir(directory1);
            AddCleanupDir(directory2);

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", tempPath),
                "Split-Path -Resolve -Leaf TestDirectory*"
            });

            Assert.AreEqual(NewlineJoin("TestDirectory1", "TestDirectory2"), result);
        }

        [Test]
        public void ResolveLeafForFullPathUsingWildcardForFileName()
        {
            string fileName1 = CreateFile(String.Empty, ".test");
            string leaf1 = Path.GetFileName(fileName1);
            string fileName2 = CreateFile(String.Empty, ".test");
            string leaf2 = Path.GetFileName(fileName2);
            string directory = Path.GetDirectoryName(fileName1);
            string wildcard = Path.Combine(directory, "*.test");

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", directory),
                string.Format("Split-Path -Resolve -Leaf '{0}'", wildcard)
            });

            Assert.AreEqual(NewlineJoin(leaf1, leaf2), result);
        }

        [Test]
        public void ResolveLeafForTwoFilesUsingRelativePathInWildcard()
        {
            string tempPath = Path.GetTempPath();
            string directory = Path.Combine(tempPath, "TestDirectory");
            Directory.CreateDirectory(directory);
            AddCleanupDir(directory);
            string fileName1 = Path.Combine(directory, "a.test");
            File.WriteAllText(fileName1, String.Empty);
            AddCleanupFile(fileName1);
            string fileName2 = Path.Combine(directory, "b.test");
            File.WriteAllText(fileName2, String.Empty);
            AddCleanupFile(fileName2);

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", directory),
                string.Format("Split-Path -Resolve -Leaf ..{0}TestDirectory{0}*.test", Path.DirectorySeparatorChar),
            });

            // Workaround ReferenceHost when running with Pash locking directory that should be deleted.
            ReferenceHost.Execute(string.Format("cd '{0}'", tempPath));

            Assert.AreEqual(NewlineJoin("a.test", "b.test"), result);
        }

        [Test]
        public void ResolveParentForTwoFilesUsingLiteralPaths()
        {
            string tempPath = Path.GetTempPath();
            string fileName1 = Path.Combine(tempPath, "File[1].txt");
            string fileName2 = Path.Combine(tempPath, "File[2].txt");
            string directory = Path.GetDirectoryName(fileName1);
            File.WriteAllText(fileName1, String.Empty);
            File.WriteAllText(fileName2, String.Empty);
            AddCleanupFile(fileName1);
            AddCleanupFile(fileName2);
            directory = GetDirectoryFullPath(directory);

            string result = ReferenceHost.Execute(new string[] {
                string.Format("cd '{0}'", tempPath),
                "Split-Path -Resolve -LiteralPath File[1].txt,File[2].txt"
            });

            Assert.AreEqual(NewlineJoin(directory, directory), result);
        }

        /// <summary>
        /// Workaround on Mac to get the full path for /var on Mac
        /// since /var is a symlink to /private/var
        /// </summary>
        string GetDirectoryFullPath(string directory)
        {
            string originalDirectory = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(directory);
                return Directory.GetCurrentDirectory();
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
    }
}

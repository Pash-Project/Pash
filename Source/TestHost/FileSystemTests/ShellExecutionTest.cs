// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using NUnit.Framework;

namespace TestHost.FileSystemTests
{
    class ShellExecutionTest : FileSystemTestBase
    {
        [Test]
        [TestCase("file.bat", "123")]
        public void FileShouldBeExecutedByAbsolutePath(string executableName, string executableResult)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            var absolutePath = Path.Combine(Path.GetFullPath(root), executableName);

            var result = TestHost.ExecuteWithZeroErrors(absolutePath);
            result.Trim().ShouldEqual(executableResult);
        }

        [Test]
        [TestCase("file.bat", "123", @".\file.bat", Ignore = true, IgnoreReason = "Ignored because of bug in command parser.")]
        [TestCase(@"directory\file.bat", "123", @"directory\file.bat")]
        public void FileShouldBeExecutedByRelativePath(string executableName, string executableResult, string command)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            Environment.CurrentDirectory = root;
            
            var result = TestHost.ExecuteWithZeroErrors(command);
            result.Trim().ShouldEqual(executableResult);
        }

        [Test]
        [TestCase("file.bat", "123", "file.bat")]
        [TestCase("file.bat", "123", "file")]
        public void FileShouldBeExecutedFromSystemPath(string executableName, string executableResult, string command)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            var path = String.Format("{0}{1}{2}", root, Path.PathSeparator, Environment.GetEnvironmentVariable("PATH"));
            Environment.SetEnvironmentVariable("PATH", path);

            var result = TestHost.ExecuteWithZeroErrors(command);
            result.Trim().ShouldEqual(executableResult);
        }
    }
}

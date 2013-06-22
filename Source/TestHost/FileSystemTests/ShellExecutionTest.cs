// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using NUnit.Framework;

namespace TestHost.FileSystemTests
{
    [TestFixture]
    public class ShellExecutionTest : FileSystemTestBase
    {
        [Test]
        [Platform("Unix")]
        [TestCase("file.sh", "123")]
        public void UnixFileShouldBeExecutedByAbsolutePath(string executableName, string executableResult)
        {
            FileShouldBeExecutedByAbsolutePath(executableName, executableResult);
        }
        
        [Test]
        [Platform("Unix")]
        [TestCase("file.sh", "123", "./file.sh", Ignore = true, IgnoreReason = "Ignored because of bug in command parser.")]
        [TestCase("directory/file.sh", "123", "directory/file.sh")]
        [TestCase("directory/file.sh", "123", "directory/file.sh 127.0.0.1", Ignore = true, IgnoreReason = "Ignored because of bug in argument parser.")]
        public void UnixFileShouldBeExecutedByRelativePath(string executableName, string executableResult, string command)
        {
            FileShouldBeExecutedByRelativePath(executableName, executableResult, command);
        }
        
        [Test]
        [Platform("Unix")]
        [TestCase("file.sh", "123", "file.sh")]
        public void UnixFileShouldBeExecutedFromSystemPath(string executableName, string executableResult, string command)
        {
            FileShouldBeExecutedFromSystemPath(executableName, executableResult, command);
        }
        
        [Test]
        [Platform("Win")]
        [TestCase("file.bat", "123")]
        public void WinFileShouldBeExecutedByAbsolutePath(string executableName, string executableResult)
        {
            FileShouldBeExecutedByAbsolutePath(executableName, executableResult);
        }
        
        [Test]
        [Platform("Win")]
        [TestCase("file.bat", "123", @".\file.bat", Ignore = true, IgnoreReason = "Ignored because of bug in command parser.")]
        [TestCase(@"directory\file.bat", "123", @"directory\file.bat")]
        [TestCase(@"directory\file.bat", "123", @"directory\file.bat 127.0.0.1", Ignore = true, IgnoreReason = "Ignored because of bug in argument parser.")]
        public void WinFileShouldBeExecutedByRelativePath(string executableName, string executableResult, string command)
        {
            FileShouldBeExecutedByRelativePath(executableName, executableResult, command);
        }

        [Test]
        [Platform("Win")]
        [TestCase("file.bat", "123", "file.bat")]
        [TestCase("file.bat", "123", "file")]
        public void WinFileShouldBeExecutedFromSystemPath(string executableName, string executableResult, string command)
        {
            FileShouldBeExecutedFromSystemPath(executableName, executableResult, command);
        }
        
        private void FileShouldBeExecutedByAbsolutePath(string executableName, string executableResult)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            var absolutePath = Path.Combine(Path.GetFullPath(root), executableName);

            CatchCommandResult(absolutePath).Trim().ShouldEqual(executableResult);
        }
        
        private void FileShouldBeExecutedByRelativePath(string executableName, string executableResult, string command)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            Environment.CurrentDirectory = root;
            
            CatchCommandResult(command).Trim().ShouldEqual(executableResult);
        }
        
        private void FileShouldBeExecutedFromSystemPath(string executableName, string executableResult, string command)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            var path = String.Format("{0}{1}{2}", root, Path.PathSeparator, Environment.GetEnvironmentVariable("PATH"));
            Environment.SetEnvironmentVariable("PATH", path);

            CatchCommandResult(command).Trim().ShouldEqual(executableResult);
        }

        /// <summary>
        /// Catches the executable result and echoes it to the output.
        /// </summary>
        /// <param name="command">Execution command.</param>
        /// <returns>Executed command result.</returns>
        private static string CatchCommandResult(string command)
        {
            var result = TestHost.ExecuteWithZeroErrors(string.Format("$result = ({0})", command), "$result");
            return result;
        }
    }
}

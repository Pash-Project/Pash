// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using System.Linq;
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
        [TestCase("directory/file.sh", "123", "directory/file.sh 127.0.0.1")]
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
        [TestCase(@"directory\file.bat", "123", @"directory\file.bat 127.0.0.1")]
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

        [Test]
        [TestCase("123", new string[] { "123" })] // simple value
        [TestCase("123 456", new string[] { "123", "456" })] // 2 arguments
        [TestCase("\"123 456\"", new string[] { "123 456" })] // quoted string
        [TestCase("\"123\",\"456\"", new string[] { "123", "456" })] // array with 2 elements is passed as 2 arguments
        public void PassArgumentsToExternalApplication(string argument, string[] expectedResults)
        {
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            string executableName = isWindows ? "file.bat" : "file.sh";
            var root = SetupExecutableWithArguments(executableName, numArguments: expectedResults.Length);

            var absolutePath = Path.Combine(Path.GetFullPath(root), executableName);
            string command = string.Format("{0} {1}", absolutePath, argument);

            var results = CatchCommandResult(command).Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < results.Length; i++)
            {
                results[i].ShouldEqual(expectedResults[i]);
            }
        }

        [Test]
        public void PassStringVariableToExternalApplication()
        {
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            string executableName = isWindows ? "file.bat" : "file.sh";
            var root = SetupExecutableWithArguments(executableName, numArguments: 1);

            var absolutePath = Path.Combine(Path.GetFullPath(root), executableName);

            var commands = new System.Collections.Generic.List<string>();
            commands.Add("$foo = \"foobar\"");
            commands.Add(string.Format("{0} $foo", absolutePath));

            CatchCommandResult(commands.ToArray()).Trim().ShouldEqual("foobar");
        }

        /// <summary>
        /// Catches the executable result and echoes it to the output.
        /// </summary>
        /// <param name="statements">statements to execute. The last statement has to return the result.</param>
        /// <returns>Executed command result.</returns>
        private static string CatchCommandResult(params string[] statements)
        {
            string[] temp = new string[ statements.Length + 1 ];

            Array.Copy( statements, temp, statements.Length - 1 );
            temp[ temp.Length - 2 ] = string.Format( "$result = ({0})", statements[ statements.Length - 1 ] );
            temp[ temp.Length - 1 ] = "$result";

            var result = TestHost.ExecuteWithZeroErrors(temp);
            return result;
        }

        protected string SetupExecutableWithArguments(string fileName, int numArguments)
        {
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            // %~1 - expands %1 removing any surrounding quotes (")
            var content = isWindows ?
                "@echo off\n" + string.Join("\n", Enumerable.Range(1, numArguments).Select(x => string.Format("echo %~{0}", x))) :
                "#!/bin/sh\n" + string.Join("\n", Enumerable.Range(1, numArguments).Select(x => string.Format("echo ${0}", x)));

            return SetupExecutable(fileName, content);
        }
    }
}

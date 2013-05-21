// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.IO;
using NUnit.Framework;

namespace TestHost.FileSystemTests
{
    class ShellExecutionTest : FileSystemTestBase
    {
        [Test]
        [TestCase("file.bat", "123", "file.bat")]
        public void FileShouldBeExecutedByAbsolutePath(string executableName, string executableResult, string command)
        {
            var root = SetupExecutableWithResult(executableName, executableResult);
            var absolutePath = Path.Combine(Path.GetFullPath(root), command);
            if (absolutePath.Contains(" "))
            {
                absolutePath = string.Format("\"{0}\"", absolutePath);
            }

            var result = TestHost.ExecuteWithZeroErrors(absolutePath);
            result.Trim().ShouldEqual(executableResult);
        }
    }
}

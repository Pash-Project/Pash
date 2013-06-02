using System;
using System.IO;
using System.Management.Automation;
using NUnit.Framework;
using Pash.Implementation;

namespace TestHost.FileSystemTests
{
    /// <summary>
    /// Tests for process execution blocking.
    /// </summary>
    public class ProcessBlockingTests : FileSystemTestBase
    {
        [Test]
        [TestCase("file.bat", true, true)]
        [TestCase("file.bat", false, true)]
        [TestCase("file.bat", null, true)]
        public void ConsoleProgramTests(string fileName, bool? flagState, bool executionState)
        {
            var root = SetupExecutableWithResult(fileName, string.Empty);
            var path = Path.Combine(root, fileName);

            AssertResult(path, flagState, executionState);
        }

        [Test]
        [Platform("Win")]
        [TestCase("notepad.exe", true, true)]
        [TestCase("notepad.exe", false, false)]
        [TestCase("notepad.exe", null, false)]
        public void GuiProgramTests(string fileName, bool? flagState, bool executionState)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var path = Path.Combine(directory, fileName);

            AssertResult(path, flagState, executionState);
        }

        private static void AssertResult(string path, bool? flagState, bool executionState)
        {
            var result = ApplicationProcessor.NeedWaitForProcess(flagState, path);
            Assert.AreEqual(executionState, result);
        }
    }
}

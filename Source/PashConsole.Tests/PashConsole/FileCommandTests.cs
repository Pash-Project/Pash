using System;
using System.Text;
using NUnit.Framework;

namespace PashConsole.Tests.PashConsole
{
    [TestFixture]
    public class FileCommandTests
    {
        [Test]
        [TestCase("-f HelloWorld.ps1")]
        [TestCase("-F HelloWorld.ps1")]
        //[TestCase("-File HelloWorld.ps1")] // NOTE: this doesn't work with the current CommandLineParser
        //[TestCase("-FILE HelloWorld.ps1")] // NOTE: this doesn't work with the current CommandLineParser
        [TestCase("--File HelloWorld.ps1")]     // NOTE: this DOES work but the Powershell on windows doesn't support this syntax
        [TestCase("--FILE HelloWorld.ps1")]
        [Platform("Mono")]
        public void TestCase(string arguments)
        {
            System.IO.File.WriteAllText("HelloWorld.ps1", "\"Hello World!\"");

            PashConsoleTestHelper.ExecutePash(arguments)
                .ShouldEqual("Hello World!" + Environment.NewLine + Environment.NewLine, "input arguments[" + arguments + "]");
        }
    }
}


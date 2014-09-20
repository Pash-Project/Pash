using System;
using System.Linq;
using System.Management.Automation.Runspaces;
using NUnit.Framework;

namespace TestHost.Cmdlets
{
    public class VerboseCommonParameterTests
    {
        [TestFixtureSetUp]
        public void LoadCmdletHelpers()
        {
            InitialSessionState sessionState = InitialSessionState.CreateDefault();
            string fileName = typeof(VerboseCommonParameterTests).Assembly.Location;
            sessionState.ImportPSModule(new string[] { fileName });
            TestHost.InitialSessionState = sessionState;
        }

        [TestFixtureTearDown]
        public void CleanInitialSessionState()
        {
            TestHost.InitialSessionState = null;
        }

        private static string NewlineJoin(params string[] parts)
        {
            return String.Join(Environment.NewLine, parts) + Environment.NewLine;
        }

        private string RunTestVerboseOutputCommand(string parameters)
        {
            string command = "Test-VerboseOutput";
            return TestHost.Execute(command + " " + parameters);
        }

        [Test]
        public void NoDefaultParametersSet()
        {
            string result = RunTestVerboseOutputCommand("-Message 'Test'");

            string expectedResult = NewlineJoin("WriteWarning: Test");
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void VerboseSet()
        {
            string result = RunTestVerboseOutputCommand("-Message 'Test' -Verbose");

            string expectedResult = NewlineJoin(
                "WriteVerbose: Test",
                "WriteWarning: Test");
            Assert.AreEqual(expectedResult, result);
        }
    }
}

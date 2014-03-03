using NUnit.Framework;
using System;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Pash.Implementation;
using System.Text;

namespace TestHost
{
    [TestFixture]
    public class ErrorTests
    {

        [TestFixtureSetUp]
        public void LoadCmdletHelpers()
        {
            InitialSessionState sessionState = InitialSessionState.Create();
            string fileName = typeof(InitialSessionStateTests).Assembly.Location;
            sessionState.ImportPSModule(new string[] { fileName });
            TestHost.InitialSessionState = sessionState;
        }

        [TestFixtureTearDown]
        public void CleanInitialSessionState()
        {
            TestHost.InitialSessionState = null;
        }


        [Test]
        public void ParseErrorAppearsInStreamAndErrorVar()
        {
            // incomplete pipe: parse error
            var result = TestHost.ExecuteWithZeroErrors(@"""foo"" |; ");
            StringAssert.Contains("ParseException", result);
            var errorVarVal = TestHost.LastUsedRunspace.ExecutionContext.GetVariableValue("error")
                               as Collection<ErrorRecord>;
            Assert.NotNull(errorVarVal, "Error var not set or not a collection of error records");
            errorVarVal.ShouldNotBeEmpty();
            Assert.True(errorVarVal[0].Exception is ParseException);
         }

        [Test]
        public void ErrorVarCanBeCleared()
        {
            // incomplete pipe: parse error
            var result = TestHost.ExecuteWithZeroErrors(new string[] {@"""foo"" |; ", "$errors.Clear()"});
            StringAssert.Contains("ParseException", result);
            var errorVarVal = TestHost.LastUsedRunspace.ExecutionContext.GetVariableValue("error")
                              as Collection<ErrorRecord>;
            errorVarVal.ShouldBeEmpty();
        }

        [Test]
        public void CreateErrorCmdletCanThrowTerminating()
        {
            var output = TestHost.ExecuteWithZeroErrors("Test-CreateError -Terminating");
            StringAssert.Contains("TestException", output);
        }


        [Test]
        public void CreateErrorCmdletCanWriteError()
        {
            var output = TestHost.ExecuteWithZeroErrors("Test-CreateError");
            StringAssert.Contains("TestException", output);
        }

        [Test]
        public void CreateErrorCmdletCanWriteObject()
        {
            var output = TestHost.ExecuteWithZeroErrors("Test-CreateError -Message 'test' -NoError");
            Assert.AreEqual("test"+ Environment.NewLine, output);
        }

        [TestCase("begin", new string[] {})]
        [TestCase("process", new string[] {"foo"})]
        [TestCase("end", new string[] {"foo", "bar"})]
        public void ThrowTerminatingBreaksPipelineInCorrectPhase(string phase, string[] expectedParts)
        {
            var errors = new StringBuilder();
            var statements_format = @"Write-Host 'foo' | Test-CreateError -Terminating -Phase {0} | Write-Host 'bar'";
            var output = TestHost.Execute(true, s => errors.Append(s), String.Format(statements_format, phase));
            var expected = String.Join(Environment.NewLine, expectedParts) + Environment.NewLine;
            StringAssert.Contains("TestException", errors.ToString());
            Assert.AreEqual(expected, output);
        }

        [TestCase("begin", new string[] {"TestException", "foo", "bar"})]
        [TestCase("process", new string[] {"foo", "TestException", "bar"})]
        [TestCase("end", new string[] {"foo", "bar", "TestException"})]
        public void WriteErrorDoesNotBreakPipeline(string phase, string[] expectedParts)
        {
            var statements_format = @"Write-Host 'foo' | Test-CreateError -Phase {0} | Write-Host 'bar'";
            var output = TestHost.ExecuteWithZeroErrors(String.Format(statements_format, phase));
            var lastpos = -1;
            // can't just compare as the "Write-Error" operation writes a whole exception
            foreach (var expected in expectedParts)
            {
                int idx = output.IndexOf(expected);
                Assert.Greater(idx, lastpos, "Expexted output not found or in wrong order!");
            }
        }
    }
}


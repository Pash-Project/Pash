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
            InitialSessionState sessionState = InitialSessionState.CreateDefault();
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
            var result = TestHost.ExecuteWithZeroErrors(new string[] {@"""foo"" |; ", "$error.Clear()"});
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
            // writes object to ui and to pipeline
            var output = TestHost.ExecuteWithZeroErrors("Test-CreateError -Message 'test' -NoError | Write-Host");
            Assert.AreEqual("test"+ Environment.NewLine + "test"+ Environment.NewLine, output);
        }

        [TestCase("begin", new string[] {})] // before anything happens
        [TestCase("process", new string[] {"foo"})] // before writing to pipeline, but after first command
        // message from ProcessRecords is processed by Out-Default
        [TestCase("end", new string[] {"foo", "foo"}, Explicit = true)] //"Pipeline currently does not process input on-demand"
        public void ThrowTerminatingBreaksPipelineInCorrectPhase(string phase, string[] expectedParts)
        {
            var errors = new StringBuilder();
            var pipeline = String.Join(" | ", new string[] {
                "Test-CreateError -Message 'foo' -NoError", // writes message to host ui and to pipeline in ProcessRecord
                // throws error in given phase and passes message to pipeline in ProcessRecord (after throwing)
                String.Format("Test-CreateError -Terminating -Phase {0}", phase)
            });
            var output = TestHost.Execute(true, s => errors.Append(s), pipeline);
            var expected = (expectedParts.Length > 0) ? 
                             String.Join(Environment.NewLine, expectedParts) + Environment.NewLine
                           : "";
            StringAssert.Contains("TestException", errors.ToString());
            Assert.AreEqual(expected, output);
        }

        // same as "end" test above, but the last output should be processed by a third command
        [Test, Ignore("Pipeline processes input currently not directly after writing")]
        public void BrokenPipelineStopsProcessingOfOutput()
        {
            var errors = new StringBuilder();
            var pipeline = String.Join(" | ", new string[] {
                "Test-CreateError -Message 'foo' -NoError",
                "Test-CreateError -Terminating -Phase End",
                "Write-Host"
            });
            var output = TestHost.Execute(true, s => errors.Append(s), pipeline);
            var expected = "foo" + Environment.NewLine + "foo" + Environment.NewLine;
            StringAssert.Contains("TestException", errors.ToString());
            Assert.AreEqual(expected, output);
        }

        [TestCase("begin")]
        [TestCase("process")]
        [TestCase("end")]
        public void WriteErrorDoesNotBreakPipeline(string phase)
        {
            var errors = new StringBuilder();
            var pipeline = String.Join(" | ", new string[] {
                "Test-CreateError -Message 'foo' -NoError", // write and pass message
                String.Format("Test-CreateError -Phase {0}", phase), // write error and pass message
                "Write-Host" // write message
            });
            var output = TestHost.Execute(true, s => errors.Append(s), pipeline);
            var expected = "foo" + Environment.NewLine + "foo" + Environment.NewLine;
            StringAssert.Contains("TestException", errors.ToString());
            Assert.AreEqual(expected, output);
        }

        [TestCase("begin", new string[] {"TestException", "foo", "foo"})]
        [TestCase("process", new string[] {"foo", "TestException", "foo"})]
        [TestCase("end", new string[] {"foo", "TestException", "foo"})]
        [Ignore("Pipeline processes input currently not directly after writing")]
        public void ErrorAppearsInCorrectOrder(string phase, string[] expectedParts)
        {
            var pipeline = String.Join(" | ", new string[] {
                "Test-CreateError -Message 'foo' -NoError", // write and pass message
                String.Format("Test-CreateError -Phase {0}", phase), // write error and pass message
                "Write-Host" // write message
            });
            var output = TestHost.ExecuteWithZeroErrors(pipeline).Split(new string[] {Environment.NewLine},
                 StringSplitOptions.None);
            Assert.AreEqual(output.Length, expectedParts.Length);
            for (int i = 0; i < expectedParts.Length; i++)
            {
                StringAssert.Contains(expectedParts[i], output[i]);
            }
        }
    }
}


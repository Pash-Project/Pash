using NUnit.Framework;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using TestPSSnapIn;

namespace ReferenceTests
{
    [TestFixture]
    public class ParametersTests
    {

        [SetUp]
        public void ImportTestInvokeScriptCmdlet()
        {
            InitialSessionState sessionState = InitialSessionState.CreateDefault();
            var fileName = typeof(TestCommand).Assembly.Location;
            sessionState.ImportPSModule(new string[] { fileName });
            ReferenceHost.InitialSessionState = sessionState;
        }

        [TearDown]
        public void ResetInitialSessionState()
        {
            //necessarry as ReferenceHost is (unfortunately) used in a static way
            ReferenceHost.InitialSessionState = null;
        }

        private string OutputString(string[] parts)
        {
            return String.Join(Environment.NewLine, parts) + Environment.NewLine;
        }

        [Test]
        public void NoMandatoriesWithoutArgsTest()
        {
            var cmd = "Test-NoMandatories";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("Reversed:  " + Environment.NewLine, res);
        }

        [TestCase("'foo'", "Correct: 1 2")]
        [TestCase("12", "Reversed: 1 2")]
        public void ParameterSetSelectionByPipelineTest(string pipeInput, string expected)
        {
            var cmd = pipeInput + " | Test-NoMandatories -One '1' -Two '2'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, res);
        }

        [TestCase("-RandomString '4'", "Correct: 1 2")]
        [TestCase("-RandomInt 4", "Reversed: 2 1")]
        public void BindingByPositionWithChosenParameterSet(string parameter, string expected)
        {
            var cmd = String.Format("Test-NoMandatories {0} '1' '2'", parameter);
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, res);
        }

        [TestCase("'foo'", "Correct: 2 1")] // right set was chosen, but positional bound by defualt set
        [TestCase("12", "Reversed: 2 1")]
        public void PositionalParametersBoundByDefaultSetIfUnsure(string pipeInput, string expected)
        {
            var cmd = pipeInput + " | Test-NoMandatories '1' '2'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, res);
        }

        [Test]
        public void OnlyOneParameterSetCanBeActive()
        {
            var cmd = "Test-NoMandatories -RandomString 'foo' -RandomInt 2";
            var ex = Assert.Throws(typeof(ParameterBindingException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            ) as ParameterBindingException;
            StringAssert.Contains("Ambiguous", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void ParameterSetWithoutMandatoriesIsNotChosenOverDefaultWithMandatory()
        {
            var cmd = "Test-MandatoryInOneSet 'works'";
            var ex = Assert.Throws(typeof(ParameterBindingException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            ) as ParameterBindingException;
            StringAssert.Contains("Missing", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void TwoParameterSetsWithSameArgumentsAreNotAmbiguous()
        {
            var cmd = "Test-MandatoryInOneSet 'works' 'foo'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("works" + Environment.NewLine, res);
        }

        [Test]
        public void AmbiguousErrorWithoutDefaultParameterSet()
        {
            var cmd = "Test-NoDefaultSet 'works'";
            var ex = Assert.Throws(typeof(ParameterBindingException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            ) as ParameterBindingException;
            StringAssert.Contains("Ambiguous", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void TooManyPositionalParametersShouldThrow()
        {
            var cmd = "Test-MandatoryInOneSet 'foo' 'bar' 'baz' 'bla'";
            var ex = Assert.Throws(typeof(ParameterBindingException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            ) as ParameterBindingException;
            StringAssert.Contains("PositionalParameterNotFound", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void PositionalParameterAfterSwitchWorks()
        {
            var cmd = "Test-SwitchAndPositional -Switch 'test'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("test" + Environment.NewLine, res);
        }

        // TODO: move to pipeline tests
        [Test]
        public void AllPhasesAreRunAsSingleCommand()
        {
            var cmd = "Test-CmdletPhases";
            var res = ReferenceHost.Execute(cmd);
            var expected = OutputString(new string[] { "Begin", "Process", "End" });
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NullAsInputOfFirstCommandDoesInvokeProcessRecords()
        {
            var cmd = "$null | Test-CmdletPhases";
            var res = ReferenceHost.Execute(cmd);
            var expected = OutputString(new string[] { "Begin", "Process", "End" });
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NoPipelineInputToSecondCommandInPipelineSkipsProcessRecords()
        {
            var cmd = "Test-Dummy | Test-CmdletPhases";
            var res = ReferenceHost.Execute(cmd);
            var expected = OutputString(new string[] { "Begin", "End" });
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void CmdletParameterWithSameAliasDontWork()
        {
            var cmd = "Test-SameAliases";

            Assert.Throws(typeof(MetadataException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            );
        }

        [Test]
        public void NonExisitingDefaultParameterSetDoesntInfluenceExecution()
        {
            // TODO: sburnicki : find out which ParameterSetName is used then?
            var cmd = "Test-DefaultParameterSetDoesntExist";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("works" + Environment.NewLine, res);
        }

        [Test]
        public void UseParameterByAmbiguousNamePrefixThrows()
        {
            var cmd = "Test-MandatoryInOneSet -TestP 'foo'"; // could be TestParam or TestParam2 
            var ex = Assert.Throws(typeof(ParameterBindingException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            ) as ParameterBindingException;
            StringAssert.Contains("Ambiguous", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void UseParameterByUnambiguousNamePrefixWorks()
        {
            var cmd = "Test-NoMandatories -One 1 -Tw 2"; // note Tw instead of "Two"
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("Reversed: 1 2" + Environment.NewLine, res);
        }

        [Test]
        public void NoDefaultSetAndPositionalsResultsInAmbiguousError()
        {
            var cmd = "Test-NoDefaultSetAndTwoPositionals foo";
            var ex = Assert.Throws(typeof(ParameterBindingException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            ) as ParameterBindingException;
            StringAssert.Contains("Ambiguous", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void CmdletWithoutParametersWorks()
        {
            var cmd = "Test-NoParameters";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("works" + Environment.NewLine, res);
        }
    }
}


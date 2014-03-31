using NUnit.Framework;
using System;
using System.Management.Automation;
using TestPSSnapIn;

namespace ReferenceTests
{
    [TestFixture]
    public class ParametersTests : ReferenceTestBase
    {

        [SetUp]
        public void ImportTestInvokeScriptCmdlet()
        {
            ReferenceHost.ImportModules(new string[] {  typeof(TestCommand).Assembly.Location });
        }

        [TearDown]
        public void ResetInitialSessionState()
        {
            ReferenceHost.ImportModules(null);
        }

        [Test]
        public void NoMandatoriesWithoutArgsTest()
        {
            var cmd = CmdletName(typeof(TestNoMandatoriesCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("Reversed:  " + Environment.NewLine, res);
        }

        [TestCase("'foo'", "Correct: 1 2")]
        [TestCase("12", "Reversed: 1 2")]
        public void ParameterSetSelectionByPipelineTest(string pipeInput, string expected)
        {
            var cmd = pipeInput + " | " + CmdletName(typeof(TestNoMandatoriesCommand)) + " -One '1' -Two '2'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, res);
        }

        [TestCase("-RandomString '4'", "Correct: 1 2")]
        [TestCase("-RandomInt 4", "Reversed: 2 1")]
        public void BindingByPositionWithChosenParameterSet(string parameter, string expected)
        {
            var cmd = String.Format(CmdletName(typeof(TestNoMandatoriesCommand)) + " {0} '1' '2'", parameter);
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, res);
        }

        [TestCase("'foo'", "Correct: 2 1")] // right set was chosen, but positional bound by defualt set
        [TestCase("12", "Reversed: 2 1")]
        public void PositionalParametersBoundByDefaultSetIfUnsure(string pipeInput, string expected)
        {
            var cmd = pipeInput + " | " + CmdletName(typeof(TestNoMandatoriesCommand)) + " '1' '2'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, res);
        }

        [Test]
        public void OnlyOneParameterSetCanBeActive()
        {
            var cmd = CmdletName(typeof(TestNoMandatoriesCommand)) +" -RandomString 'foo' -RandomInt 2";
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
            var cmd = CmdletName(typeof(TestMandatoryInOneSetCommand)) + " 'works'";
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
            var cmd = CmdletName(typeof(TestMandatoryInOneSetCommand)) + " 'works' 'foo'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("works" + Environment.NewLine, res);
        }

        [Test]
        public void AmbiguousErrorWithoutDefaultParameterSet()
        {
            var cmd = CmdletName(typeof(TestNoDefaultSetCommand)) + " 'works'";
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
            var cmd = CmdletName(typeof(TestMandatoryInOneSetCommand)) + " 'foo' 'bar' 'baz' 'bla'";
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
            var cmd = CmdletName(typeof(TestSwitchAndPositionalCommand)) + " -Switch 'test'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("test" + Environment.NewLine, res);
        }

        [Test]
        public void CmdletParameterWithSameAliasDontWork()
        {
            var cmd = CmdletName(typeof(TestSameAliasesCommand));

            Assert.Throws(typeof(MetadataException),
                delegate()
                {
                    ReferenceHost.Execute(cmd);
                }
            );
        }

        [Test]
        public void NonExisitingDefaultParameterSetIsEmptyPatameterSet()
        {
            var cmd = CmdletName(typeof(TestDefaultParameterSetDoesntExistCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("works" + Environment.NewLine, res);
        }

        [Test]
        public void UseParameterByAmbiguousNamePrefixThrows()
        {
            var cmd = CmdletName(typeof(TestMandatoryInOneSetCommand)) + " -TestP 'foo'"; // could be TestParam or TestParam2 
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
            var cmd = CmdletName(typeof(TestNoMandatoriesCommand)) + " -One 1 -Tw 2"; // note Tw instead of "Two"
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("Reversed: 1 2" + Environment.NewLine, res);
        }

        [Test]
        public void NoDefaultSetAndPositionalsResultsInAmbiguousError()
        {
            var cmd = CmdletName(typeof(TestNoDefaultSetAndTwoPositionalsCommand)) + " foo";
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
            var cmd = CmdletName(typeof(TestNoParametersCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual("works" + Environment.NewLine, res);
        }
    }
}


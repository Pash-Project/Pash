// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Management.Automation;
using System.Linq;
using TestPSSnapIn;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class CmdletParameterTests : ReferenceTestBaseWithTestModule
    {
        [Test]
        public void NoMandatoriesWithoutArgsTest()
        {
            var cmd = CmdletName(typeof(TestNoMandatoriesCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("Reversed:  "), res);
        }

        [Test]
        public void ParameterIsNotMandatoryByDefault()
        {
            var cmd = CmdletName(typeof(TestParamIsNotMandatoryByDefaultCommand)); // should work without param
            var res = ReferenceHost.RawExecute(cmd);
            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0], Is.Null);
        }

        [Test]
        public void CmdletWithoutProvidedMandatoryThrows()
        {
            var cmd = CmdletName(typeof(TestWithMandatoryCommand));
            var ex = Assert.Throws<ParameterBindingException>(delegate {
                ReferenceHost.Execute(cmd);
            });
            StringAssert.Contains("Missing", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [TestCase("'foo'", "Correct: 1 2")]
        [TestCase("12", "Reversed: 1 2")]
        public void ParameterSetSelectionByPipelineTest(string pipeInput, string expected)
        {
            var cmd = pipeInput + " | " + CmdletName(typeof(TestNoMandatoriesCommand)) + " -One '1' -Two '2'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), res);
        }

        [Test]
        public void ParameterSetSelectionWithOneMandatoryParameterByPipeline()
        {
            var cmd = "2 | " + CmdletName(typeof(TestOneMandatoryParamByPipelineSelectionCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("Integer", "Integer"), res);
        }

        [TestCase("1", new [] { "Message", "Integer" })]
        [TestCase("'foo'", new [] { "Message", "Message" })]
        public void ParameterSetSelectionWithMandatoryParameterByPipeline(string pipeInput, string[] expected)
        {
            var cmd = pipeInput + " | " + CmdletName(typeof(TestMandatoryParamByPipelineSelectionCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), res);
        }

        [TestCase("1", new [] { "__AllParameterSets", "Integer" })]
        [TestCase("'foo'", new [] { "__AllParameterSets", "Message" })]
        public void ParameterSetSelectionWithMandatoryParameterByPipelineWithoutDefault(string pipeInput, string[] expected)
        {
            var cmd = pipeInput +" | " + CmdletName(typeof(TestMandatoryParamByPipelineSelectionWithoutDefaultCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), res);
        }

        [TestCase("-RandomString '4'", "Correct: 1 2")]
        [TestCase("-RandomInt 4", "Reversed: 2 1")]
        public void BindingByPositionWithChosenParameterSet(string parameter, string expected)
        {
            var cmd = String.Format(CmdletName(typeof(TestNoMandatoriesCommand)) + " {0} '1' '2'", parameter);
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), res);
        }

        [TestCase("'foo'", "Correct: 2 1")] // right set was chosen, but positional bound by defualt set
        [TestCase("12", "Reversed: 2 1")]
        public void PositionalParametersBoundByDefaultSetIfUnsure(string pipeInput, string expected)
        {
            var cmd = pipeInput + " | " + CmdletName(typeof(TestNoMandatoriesCommand)) + " '1' '2'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), res);
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
        public void ArgumentsCanBeBoundExplicitlyWithSpace()
        {
            var cmd = CmdletName(typeof(TestWriteTwoMessagesCommand)) + " -Msg1: -Msg2 -Msg2: 'foo'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("1: -Msg2, 2: foo"), res);
        }

        [Test]
        public void TwoParameterSetsWithSameArgumentsAreNotAmbiguous()
        {
            var cmd = CmdletName(typeof(TestMandatoryInOneSetCommand)) + " 'works' 'foo'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("works"), res);
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
            Assert.AreEqual(NewlineJoin("test"), res);
        }

        [TestCase("$true", true)]
        [TestCase("$false", false)]
        [TestCase("$null", true)]
        [TestCase("0.0", false)]
        [TestCase("0.01", true)]
        public void SwitchParameterWithExplicitValue(string value, bool expected)
        {
            var cmd = CmdletName(typeof(TestSwitchParameterCommand)) + " -Switch:" + value;
            var res = ReferenceHost.RawExecute(cmd);
            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0].BaseObject, Is.EqualTo(expected));
        }

        [Test]
        public void NamedParameterOnlyNeedsCaseInsensitiveStart()
        {
            var cmd = CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand)) + " -f 'test'";
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("test "), res);
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
            Assert.AreEqual(NewlineJoin("works"), res);
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
            Assert.AreEqual(NewlineJoin("Reversed: 1 2"), res);
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
            Assert.AreEqual(NewlineJoin("works"), res);
        }

        [Test]
        public void DefaultSetIsAllParametersAndTwoParameterSetsWhenNoParameterPassedShouldExecuteCmdlet()
        {
            var cmd = CmdletName(typeof(TestDefaultSetIsAllParameterSetAndTwoParameterSetsCommand));
            var res = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("First: null"), res);
        }

        [Test]
        public void TwoParameterAmbiguousSetsWhenNoParameterPassedShouldNotExecuteCmdlet()
        {
            var cmd = CmdletName(typeof(TestTwoAmbiguousParameterSetsCommand));
            var ex = Assert.Throws<ParameterBindingException>(() =>
                {
                    ReferenceHost.Execute(cmd);
                }
            );
            StringAssert.Contains("Ambiguous", ex.ErrorRecord.FullyQualifiedErrorId);
        }

        [Test]
        public void CmdletCanTakeParametersByPropertyName()
        {
            var cmd = "new-object psobject -property @{foO='abc'; bAr='def'} | "
                + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand));
            var expected = "abc def" + Environment.NewLine;
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void CmdletPipeParamByPropertyNameCantBePartial()
        {
            var cmd = "new-object psobject -property @{f='abc'; b='def'} | "
                + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand));
            Assert.Throws<MethodInvocationException>(() => {
                ReferenceHost.Execute(cmd);
            });
        }

        [Test]
        public void CmdletPipeParamByPropertyNameCanBeAlias()
        {
            var cmd = "new-object psobject -property @{baz='abc'; b='def'} | "
                + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand));
            var expected = "abc " + Environment.NewLine;
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CmdletPipeParamByPropertyNameCanOnlyProvideOptional()
        {
            var cmd = "new-object psobject -property @{bar='def'} | "
                + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand))
                + " -Foo 'a'";
            var expected = "a def" + Environment.NewLine;
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        [TestCase(" -Foo 'a'", "a def")]
        [TestCase(" -Bar 'a'", "abc a")]
        public void CmdletPipeParamByPropertyNameIgnoresAlreadyBound(string parameters, string expected)
        {
            var cmd = "new-object psobject -property @{foo='abc'; bar='def'} | "
                + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand))
                + parameters;
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [Test]
        public void CmdletPipeParamByPropertyFailsIfAllAreAlreadyBound()
        {
            var cmd = "new-object psobject -property @{foo='abc'; bar='def'} | "
                + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand))
                + " -Foo 'a' -Bar 'a'";
            Assert.Throws<MethodInvocationException>(() => {
                ReferenceHost.Execute(cmd);
            });
        }

        [Test, Explicit("To be honest: I don't understand why PS writes the last output AND still throws and excpetion")]
        public void CmdletPipeParamByPropertyCanProcessMultipleButThrowsOnError()
        {
            var cmd = NewlineJoin(new string[] {
                "$a = new-object psobject -property @{foo='abc'; bar='def'}",
                "$b = new-object psobject -property @{foo='ghi'}",
                "$c = new-object psobject -property @{bar='jkl'}",
                "$d = new-object psobject -property @{foo='mno'; bar='jkl'}",
                "@($a, $b, $c, $d) | " + CmdletName(typeof(TestParametersByPipelinePropertyNamesCommand))
            });
            var expected = NewlineJoin(new string[] {
                "abc def",
                "ghi ",
                "mno jkl"
            });
            Assert.Throws<MethodInvocationException>(() =>
            {
                ReferenceHost.Execute(cmd);
            });
            // stuff before and after third object should work
            Assert.AreEqual(expected, ReferenceHost.LastResults);
        }

        [Test]
        public void ParameterSelectionMakesDefaultUneligible()
        {
            var cmd = CmdletName(typeof(TestParameterInTwoSetsButNotDefaultCommand)) + " -Custom 'foo'";
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("Custom1")));
        }

        [Test]
        public void CmdletPipeParamPropertyPreferredOverConversion()
        {
            var cmdletName = CmdletName(typeof(TestParametersByPipelineWithPropertiesAndConversionCommand));
            var createObjectCmdlet = CmdletName(typeof(TestCreateFooMessageObjectCommand));
            var cmd = NewlineJoin(new string[]{
                "$a = " + createObjectCmdlet + " hello",
                "$a | " + cmdletName
            });
            var expected = "hello" + Environment.NewLine;
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerboseCommonParameterAvailableFromGetCommandCmdlet()
        {
            CmdletInfo info = ReferenceHost.RawExecute("Get-Command")
                .Select(psObject => psObject.BaseObject as CmdletInfo)
                .FirstOrDefault(cmdletInfo => (cmdletInfo != null) && (cmdletInfo.Name == "Get-Command"));
            CommandParameterSetInfo parameterSetInfo = info.ParameterSets[0];
            CommandParameterInfo verboseParameter = parameterSetInfo.Parameters.FirstOrDefault(parameter => parameter.Name == "Verbose");

            Assert.IsNotNull(verboseParameter);
            Assert.IsTrue(verboseParameter.Aliases.Contains("vb"));
        }
    }
}


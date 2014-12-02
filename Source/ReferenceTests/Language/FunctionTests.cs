// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class FunctionTests : ReferenceTestBase
    {
        [Test]
        public void FunctionDeclarationWithoutParameterList()
        {
            Assert.DoesNotThrow(
                delegate() { ReferenceHost.Execute("function f() { 'x' }"); }
            );
        }

        [TestCase("function f { param($test=(get-location).path)")]
        [TestCase("function f ($test=(get-location).path) {")]
        public void FunctionParamsDefaultValueCanBeExpression(string funStart)
        {
            var cmd = NewlineJoin(
                funStart,
                "$test",
                "}",
                "f");
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(Environment.CurrentDirectory), result);
        }

        [TestCase("function f($a, $b) { ")]
        [TestCase("function f { param($a, $b); ")]
        public void FunctionWithParameters(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $b; }; f 1 2", 1, 2);
        }

        [TestCase("function f($a, $b) { ")]
        [TestCase("function f { param($a, $b); ")]
        public void FunctionWithoutPassedParameters(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $b; }; f", null, null);
        }

        [TestCase("function f($a, $b) { ")]
        [TestCase("function f { param($a, $b); ")]
        public void FunctionWithNamedParameters(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $b; }; f -b 1 -a 2", 2, 1);
        }

        [TestCase("function f($a, $b) { ")]
        [TestCase("function f { param($a, $b); ")]
        public void FunctionWithExplicitlyNamedParameters(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $b; }; f -b: 1 -a: 2", 2, 1);
        }

        [TestCase("function f($a, $b) { ")]
        [TestCase("function f { param($a, $b); ")]
        public void FunctionWithMoreArgsInArgsVar(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $b; $args; }; f -b 1 '-a' -d -e", null, 1, "-a", "-d", "-e");
        }

        [Test]
        public void FunctionWithBothParenthesisAndParamBlockThrows()
        {
            Assert.Throws<ParseException>(delegate {
                ReferenceHost.Execute("function f($a, $b) { param($a, $b); };");
            });
        }
    }
}


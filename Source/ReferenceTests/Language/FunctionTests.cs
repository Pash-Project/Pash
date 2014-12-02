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

        [Test]
        public void FunctionParamsParamBlockDefaultValueCanBeExpression()
        {
            var cmd = NewlineJoin(
                "function f { param($test=(get-location).path)",
                "$test",
                "}",
                "f");
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(Environment.CurrentDirectory), result);
        }

        [Test]
        public void FunctionParamsParenthesisDefaultValueCanBeExpression()
        {
            var cmd = NewlineJoin(
                "function f ($test=(get-location).path) {",
                "$test",
                "}",
                "f");
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(Environment.CurrentDirectory), result);
        }

        [Test]
        public void FunctionWithParametersInParanthesis()
        {
            ExecuteAndCompareTypedResult("function f($a, $b) { $a; $b; }; f 1 2", 1, 2);
        }

        [Test]
        public void FunctionWithParametersInParamBlock()
        {
            ExecuteAndCompareTypedResult("function f { param($a, $b); $a; $b; }; f 1 2", 1, 2);
        }

        [Test]
        public void FunctionWithoutPassedParameters()
        {
            ExecuteAndCompareTypedResult("function f($a, $b) { $a; $b; }; f", null, null);
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


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

        [Test]
        public void FunctionParamsBlockAfterNewline()
        {
            var cmd = NewlineJoin(
                "function addNums {",
                "",
                "param (",
                "$a,",
                "$b",
                ")",
                "$a + $b",
                "}",
                "addNums 2 5");
            ExecuteAndCompareTypedResult(cmd, 7);
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
            ExecuteAndCompareTypedResult(funStart + "$a; $b; $args; }; f -b 1 '-a' -d -e", "-a", 1, "-d", "-e");
        }

        [TestCase("function f($a, $b=3, $c=4) { ")]
        [TestCase("function f { param($a, $b=3, $c=4); ")]
        public void FunctionCanHaveMultipleDefautValues(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a + $b + $c }; f 1 -c 1", 5);
        }

        [TestCase("function f($a, $b) { ")]
        [TestCase("function f { param($a, $b); ")]
        public void FunctionsUndefinedNamedArgsAreInArgsVar(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $b; $args; }; f -c 1 -d 'val' -e", null, null, "-c", 1, "-d", "val", "-e");
        }

        [TestCase("function f($a) { ")]
        [TestCase("function f { param($a); ")]
        public void FunctionsUndefinedNamedArgsKeepCorrectOrder(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + "$a; $args; }; f 1 2 -d 'val' -e 3 4", 1, 2, "-d", "val", "-e", 3, 4);
        }

        [TestCase("function f($a='f') { ")]
        [TestCase("function f { param($a='f'); ")]
        public void FunctionsNamedUnsetParameterThrows(string funStart)
        {
            Assert.Throws<ParameterBindingException>(delegate {
                ReferenceHost.Execute(funStart + "}; f -a");
            });
        }

        [TestCase("function f($a=1,2) { ")]
        [TestCase("function f { param($a=1,2); ")]
        public void FunctionWithLiteralArrayAsDefaultParameterThrows(string funStart)
        {
            Assert.Throws<ParseException>(delegate {
                ReferenceHost.Execute(funStart + "};");
            });
        }

        [TestCase("function f($a='f') { ")]
        [TestCase("function f { param($a='f'); ")]
        public void FunctionsNamedParameterCanTakeUnusedParamName(string funStart)
        {
            ExecuteAndCompareTypedResult(funStart + " $a; }; f -a -b", "-b");
        }

        [TestCase("function f($a='f') { ", "-a: $null")]
        [TestCase("function f($a='f') { ", "-a $null")]
        [TestCase("function f { param($a='f'); ", "-a: $null")]
        [TestCase("function f { param($a='f'); ", "-a $null")]
        public void FunctionsNamedParameterCanTakeExplicitNull(string funStart, string arg)
        {
            ExecuteAndCompareTypedResult(funStart + " $a; }; f " + arg, null);
        }

        [TestCase("function f($a='f') { ")]
        [TestCase("function f { param($a='f'); ")]
        public void FunctionsNamedParameterCantTakeUnusedButSetParam(string funStart)
        {
            Assert.Throws<ParameterBindingException>(delegate {
                ReferenceHost.Execute(funStart + " $a; }; f -a -b: foo");
            });
        }

        [TestCase("function f($a, $A) { ")]
        [TestCase("function f { param($a, $A); ")]
        public void DuplicateParameterNamesInvariantCaseThrowParseError(string funStart)
        {
            Assert.Throws<ParseException>(delegate {
                ReferenceHost.Execute(funStart + " $a; }");
            });
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


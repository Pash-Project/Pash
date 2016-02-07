// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class GetVariableTests : ReferenceTestBase
    {
        [Test]
        public void ByName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "(Get-Variable foo).Value"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NameParameter()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "(Get-Variable -name foo).Value"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NameIsCaseInsensitive()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "(Get-Variable -name FOO).Value"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void GetTwoVariables()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = '1'",
                "$b = '2'",
                "Get-Variable 'a','b' | % { $_.value }"
            });

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void GetTwoVariablesUsingNamesPassedOnPipeline()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = '1'",
                "$b = '2'",
                "'a','b' | Get-Variable | % { $_.value }"
            });

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void NameAndValueOnlyParameter()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "Get-Variable foo -ValueOnly"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void FunctionLocalScopeVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Get-Variable test -Scope local -valueonly; }",
                "foo"
            });

            Assert.AreEqual("test-local" + Environment.NewLine, result);
        }

        [Test]
        public void FunctionGlobalScopeVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Get-Variable test -Scope global -valueonly; }",
                "foo"
            });

            Assert.AreEqual("test-global" + Environment.NewLine, result);
        }

        [Test]
        public void NoParametersReturnsAllParameters()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "Get-Variable| % { $_.Value }"
            });

            StringAssert.Contains("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NoNameAndValueOnlyReturnsAllParameterValues()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "Get-Variable -ValueOnly"
            });

            StringAssert.Contains("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NoNameAndLocalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Get-Variable -Scope local; }",
                "foo | ? { $_.Name -eq 'test' } | % { $_.Value }"
            });

            Assert.AreEqual("test-local" + Environment.NewLine, result);
        }

        [Test]
        public void NoNameAndGlobalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Get-Variable -Scope global; }",
                "foo | ? { $_.Name -eq 'test' } | % { $_.Value }"
            });

            Assert.AreEqual("test-global" + Environment.NewLine, result);
        }
    }
}

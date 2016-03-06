// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
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

        [Test]
        public void FunctionNoScopeSpecifiedUsesLocalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Get-Variable test -valueonly; }",
                "foo"
            });

            Assert.AreEqual("test-local" + Environment.NewLine, result);
        }

        [Test]
        public void HostVariableName()
        {
            string result = ReferenceHost.Execute("(Get-Variable host).Name");

            Assert.AreEqual("Host" + Environment.NewLine, result);
        }

        [Test]
        public void IncludeTwoVariables()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testabc = 'abc'",
                "$testbar = 'bar'",
                "$testfoo = 'foo'",
                "gv -Include testfoo,testbar -valueonly"
            });

            Assert.AreEqual(NewlineJoin("bar", "foo"), result);
        }

        [Test]
        public void ExcludeTwoVariables()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testfoo = 'foo'",
                "$testbar = 'bar'",
                "$testabc = 'abc'",
                "gv -exclude testfoo,testbar -valueonly"
            });

            StringAssert.Contains("abc" + Environment.NewLine, result);
            StringAssert.DoesNotContain("foo", result);
            StringAssert.DoesNotContain("bar", result);
        }

        [Test]
        public void IncludeIsCaseInsensitive()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testbar = 'bar'",
                "$testfoo = 'foo'",
                "Get-Variable -Include TESTFOO -valueonly"
            });

            Assert.AreEqual("foo" + Environment.NewLine, result);
        }

        [Test]
        public void ExcludeIsCaseInsensitive()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testfoo = 'foo'",
                "$testbar = 'bar'",
                "Get-Variable -exclude TESTFOO -valueonly"
            });

            StringAssert.Contains("bar" + Environment.NewLine, result);
            StringAssert.DoesNotContain("foo", result);
        }

        [Test]
        public void NameAndIncludeSpecified()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testfoo = 'foo'",
                "$testbar = 'bar'",
                "Get-Variable testfoo -include testbar -valueonly"
            });

            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void NameAndExcludeSpecified()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testfoo = 'foo'",
                "$testbar = 'bar'",
                "Get-Variable testfoo -exclude TESTFOO -valueonly"
            });

            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void NameIsWildcard()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testbar = 'bar'",
                "$testfoo = 'foo'",
                "Get-Variable test* -valueonly"
            });

            Assert.AreEqual(NewlineJoin("bar", "foo"), result);
        }

        [Test]
        public void NameIsWildcardAndLocalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Get-Variable test* -Scope local -valueonly; }",
                "foo"
            });

            Assert.AreEqual("test-local" + Environment.NewLine, result);
        }

        [Test]
        [Explicit("Does not work with Pash. Global variable should not be returned.")]
        public void NameIsWildcardAndLocalScopeWithDifferentVariableNamesInLocalAndGlobalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test1 = 'test-global'",
                "function foo { $test2 = 'test-local'; Get-Variable test* -Scope local -valueonly; }",
                "foo"
            });

            Assert.AreEqual("test-local" + Environment.NewLine, result);
        }

        [Test]
        public void NameIsWildcardAndGlobalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test1 = 'test-global'",
                "function foo { $test2 = 'test-local'; Get-Variable test* -Scope global -valueonly; }",
                "foo"
            });

            Assert.AreEqual("test-global" + Environment.NewLine, result);
        }

        [Test]
        public void IncludeIsWildcard()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testbar = 'bar'",
                "$testfoo = 'foo'",
                "Get-Variable -include TESTF* -valueonly"
            });

            Assert.AreEqual("foo" + Environment.NewLine, result);
        }

        [Test]
        public void ExcludeIsWildcard()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testabc = 'abc'",
                "$testbar = 'bar'",
                "$testfoo = 'foo'",
                "Get-Variable -include test* -exclude TESTA* -valueonly"
            });

            Assert.AreEqual(NewlineJoin("bar", "foo"), result);
        }

        [Test]
        public void UnknownNameCausesError()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Get-Variable unknownvariable");
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'unknownvariable'.", error.Exception.Message);
            Assert.AreEqual("VariableNotFound,Microsoft.PowerShell.Commands.GetVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("unknownvariable", error.TargetObject);
            Assert.IsInstanceOf<ItemNotFoundException>(error.Exception);
            Assert.AreEqual("Get-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.ObjectNotFound, error.CategoryInfo.Category);
            Assert.AreEqual("ItemNotFoundException", error.CategoryInfo.Reason);
            Assert.AreEqual("unknownvariable", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
        }

        [Test]
        public void WildcardEscapedToGetQuestionMarkVariable()
        {
            string result = ReferenceHost.Execute("(Get-Variable '`?').Name");

            Assert.AreEqual("?" + Environment.NewLine, result);
        }

        [Test]
        public void WildcardEscapedAndScopeToGetQuestionMarkVariable()
        {
            string result = ReferenceHost.Execute("(Get-Variable '`?' -Scope global).Name");

            Assert.AreEqual("?" + Environment.NewLine, result);
        }

        [Test]
        public void UnknownVariableWithWildcardEscaped()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                 ReferenceHost.Execute("Get-Variable '`?unknown`?'");
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name '`?unknown`?'.", error.Exception.Message);
            Assert.AreEqual("`?unknown`?", error.TargetObject);
            Assert.AreEqual("`?unknown`?", error.CategoryInfo.TargetName);
        }

        [Test]
        public void VariablesSortedByName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$testfoo = 'foo'",
                "$testbar = 'bar'",
                "$testzzz = 'bar'",
                "$testabc = 'abc'",
                "Get-Variable -include test* | % { $_.Name }"
            });

            Assert.AreEqual(NewlineJoin("testabc", "testbar", "testfoo", "testzzz"), result);
        }

        [Test]
        public void GetPrivateVariableByName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name foo -visibility private",
                    "Get-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot access the variable '$foo' because it is a private variable", error.Exception.Message);
            Assert.AreEqual("VariableIsPrivate,Microsoft.PowerShell.Commands.GetVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("foo", error.TargetObject);
            Assert.IsInstanceOf<SessionStateException>(error.Exception);
            Assert.AreEqual("Get-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.PermissionDenied, error.CategoryInfo.Category);
            Assert.AreEqual("SessionStateException", error.CategoryInfo.Reason);
            Assert.AreEqual("foo", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
        }

        [Test]
        public void GetTwoPrivateVariablesByName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name foo -visibility private",
                    "New-Variable -name bar -visibility private",
                    "Get-Variable foo,bar"
                });
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            Assert.AreEqual(2, ReferenceHost.GetLastRawErrorRecords().Count());
            StringAssert.Contains("Cannot access the variable '$foo' because it is a private variable", error1.Exception.Message);
            StringAssert.Contains("Cannot access the variable '$bar' because it is a private variable", error2.Exception.Message);
        }

        [Test]
        public void PrivateVariableNotReturnedInWildcardSearch()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable -name foo -visibility private",
                "Get-Variable fo?"
            });

            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void GetPrivateVariableByNameAndScope()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name foo -visibility private",
                    "Get-Variable foo -scope global"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot access the variable '$foo' because it is a private variable", error.Exception.Message);
            Assert.AreEqual("VariableIsPrivate,Microsoft.PowerShell.Commands.GetVariableCommand", error.FullyQualifiedErrorId);
        }

        [Test]
        public void GetTwoPrivateVariablesByNameAndScope()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name foo -visibility private",
                    "New-Variable -name bar -visibility private",
                    "Get-Variable foo,bar -scope global"
                });
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            Assert.AreEqual(2, ReferenceHost.GetLastRawErrorRecords().Count());
            StringAssert.Contains("Cannot access the variable '$foo' because it is a private variable", error1.Exception.Message);
            StringAssert.Contains("Cannot access the variable '$bar' because it is a private variable", error2.Exception.Message);
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class ClearVariableTests : ReferenceTestBase
    {
        [Test]
        public void ByName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "Clear-Variable foo",
                "$foo -eq $null"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void NameUsingNamedParametersAndAbbreviatedCommandName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "clv -name foo",
                "$foo -eq $null"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void MultipleNames()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = 'abc'",
                "$b = 'abc'",
                "Clear-Variable a,b",
                "($a -eq $null).ToString() + \", \" + ($b -eq $null).ToString()"
            });

            Assert.AreEqual("True, True" + Environment.NewLine, result);
        }

        [Test]
        public void PassThru()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'abc'",
                "$output = Clear-Variable foo -passthru",
                "'Name=' + $output.Name + ', Value is null=' + ($output.Value -eq $null).ToString() + ', Type=' + $output.GetType().Name"
            });

            Assert.AreEqual("Name=foo, Value is null=True, Type=PSVariable" + Environment.NewLine, result);
        }

        [Test]
        public void PassThruTwoVariablesCleared()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'abc'",
                "$bar = 'abc'",
                "$v = Clear-Variable foo,bar -passthru",
                "'Names=' + $v[0].Name + ',' + $v[1].Name + ' Values are null=' + ($v[0].Value -eq $null).ToString() + ',' + ($v[1].Value -eq $null).ToString() + ' Type=' + $v.GetType().Name + ' ' + $v[0].GetType().Name"
            });

            Assert.AreEqual("Names=foo,bar Values are null=True,True Type=Object[] PSVariable" + Environment.NewLine, result);
        }

        [Test]
        public void UnknownNameCausesError()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Clear-Variable unknownvariable");
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'unknownvariable'.", error.Exception.Message);
            Assert.AreEqual("VariableNotFound,Microsoft.PowerShell.Commands.ClearVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("unknownvariable", error.TargetObject);
            Assert.IsInstanceOf<ItemNotFoundException>(error.Exception);
            Assert.AreEqual("Clear-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.ObjectNotFound, error.CategoryInfo.Category);
            Assert.AreEqual("ItemNotFoundException", error.CategoryInfo.Reason);
            Assert.AreEqual("unknownvariable", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
        }

        [Test]
        public void TwoUnknownNamesCausesTwoErrors()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Clear-Variable unknownvariable1,unknownvariable2");
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            Assert.AreEqual(2, ReferenceHost.GetLastRawErrorRecords().Count());
            StringAssert.Contains("Cannot find a variable", error1.Exception.Message);
            StringAssert.Contains("name 'unknownvariable1'.", error1.Exception.Message);
            StringAssert.Contains("Cannot find a variable", error2.Exception.Message);
            StringAssert.Contains("name 'unknownvariable2'.", error2.Exception.Message);
        }

        [Test]
        public void TryToClearReadOnlyVariable()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable foo 'bar' -option readonly",
                    "Clear-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            var sessionStateException = error.Exception as SessionStateUnauthorizedAccessException;

            Assert.AreEqual("foo", error.TargetObject);
            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", error.Exception.Message);
            Assert.IsInstanceOf<SessionStateUnauthorizedAccessException>(error.Exception);
            Assert.AreEqual("foo", sessionStateException.ItemName);
            Assert.AreEqual(SessionStateCategory.Variable, sessionStateException.SessionStateCategory);
            Assert.AreEqual("System.Management.Automation", sessionStateException.Source);
            Assert.AreEqual("Clear-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.WriteError, error.CategoryInfo.Category);
            Assert.AreEqual("SessionStateUnauthorizedAccessException", error.CategoryInfo.Reason);
            Assert.AreEqual("foo", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
            Assert.AreEqual("VariableNotWritable,Microsoft.PowerShell.Commands.ClearVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void TryToClearConstVariable()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable foo 'bar' -option constant",
                    "Clear-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", error.Exception.Message);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void TryToClearTwoConstantVariables()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable -name foo -option constant",
                    "Set-Variable -name bar -option constant",
                    "Clear-Variable -name foo,bar"
                });
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            Assert.AreEqual(2, ReferenceHost.GetLastRawErrorRecords().Count());
            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", error1.Exception.Message);
            Assert.AreEqual("Cannot overwrite variable bar because it is read-only or constant.", error2.Exception.Message);
        }

        [Test]
        public void ClearReadOnlyVariableUsingForce()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable foo 'bar' -option readonly",
                "Clear-Variable foo -force",
                "($foo -eq $null).ToString()"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void TryToClearConstantVariableWithForce()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable -name foo -option constant",
                    "Clear-Variable -name foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", error.Exception.Message);
        }

        [Test]
        public void ClearLocalScopeVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Clear-Variable test -Scope local; return $test; }",
                "$a = foo",
                "($a -eq $null).ToString() + ', ' + $test"
            });

            Assert.AreEqual("True, test-global" + Environment.NewLine, result);
        }

        [Test]
        public void ClearGlobalScopeVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test = 'test-global'",
                "function foo { $test = 'test-local'; Clear-Variable test -Scope global; return $test; }",
                "$a = foo",
                "$a + ', ' + ($test -eq $null).ToString()"
            });

            Assert.AreEqual("test-local, True" + Environment.NewLine, result);
        }

        [Test]
        public void WildcardClearsMultipleVariableValues()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = 'abc'",
                "$aa= 'abc'",
                "$ab = 'abc'",
                "$b = 'abc'",
                "Clear-Variable a*",
                "($a -eq $null).ToString() + \", \" + ($aa -eq $null).ToString() + \", \" + ($ab -eq $null).ToString() + \", \" + ($b -eq $null).ToString()"
            });

            Assert.AreEqual("True, True, True, False" + Environment.NewLine, result);
        }

        [Test]
        [Explicit("Does not work with Pash. Global variable should not be cleared.")]
        public void NameIsWildcardAndLocalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test1 = 'test-global'",
                "function foo { $test2 = 'test-local'; Clear-Variable test* -Scope local; $test2}",
                "$result = foo",
                "($result -eq $null).ToString() + ', ' + $test1"
            });

            Assert.AreEqual("True, test-global" + Environment.NewLine, result);
        }

        [Test]
        public void NameIsWildcardAndGlobalScope()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$test1 = 'test-global'",
                "function foo { $test2 = 'test-local'; Clear-Variable test* -Scope global; $test2}",
                "$result = foo",
                "($test1 -eq $null).ToString() + ', ' + $result"
            });

            Assert.AreEqual("True, test-local" + Environment.NewLine, result);
        }

        [Test]
        public void PrivateVariableWithWildcardNoErrorsReportedAndPrivateVariableNotCleared()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$createdVariable = New-Variable -name foo1 -value 'foo1' -visibility private -passthru",
                "$foo2 = 'abc'",
                "Clear-Variable foo?",
                "$createdVariable.Value + ', ' + ($foo2 -eq $null).ToString()"
            });

            Assert.AreEqual("foo1, True" + Environment.NewLine, result);
        }

        [Test]
        public void UnknownVariableWithWildcardEscaped()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Clear-Variable '`?unknown`?'");
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name '`?unknown`?'.", error.Exception.Message);
            Assert.AreEqual("`?unknown`?", error.TargetObject);
            Assert.AreEqual("`?unknown`?", error.CategoryInfo.TargetName);
        }

        [Test]
        public void WildcardEscapedToClearVariableWithWildcardInName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable -name 'a?b' -value 'a'",
                "Clear-Variable a`?b",
                "($a?b -eq $null).ToString()"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void IncludeNamesByWildcard()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$aa = 'aa'",
                "$ba = 'ba'",
                "Clear-Variable aa,ba -include b*",
                "$aa + \", \" + ($ba -eq $null).ToString()"
            });

            Assert.AreEqual("aa, True" + Environment.NewLine, result);
        }

        [Test]
        public void ExcludeNamesByWildcard()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$aa = 'aa'",
                "$ba = 'ba'",
                "Clear-Variable aa,ba -exclude b*",
                "($aa -eq $null).ToString() + \", \" + $ba"
            });

            Assert.AreEqual("True, ba" + Environment.NewLine, result);
        }

        [Test]
        public void WildcardAndExcludeOneVariableName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$aa = 'aa'",
                "$ab = 'ab'",
                "Clear-Variable a* -exclude aa",
                "$aa + \", \" + ($ab -eq $null).ToString()"
            });

            Assert.AreEqual("aa, True" + Environment.NewLine, result);
        }
    }
}

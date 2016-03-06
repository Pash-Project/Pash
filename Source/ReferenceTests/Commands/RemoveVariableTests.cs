// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class RemoveVariableTests : ReferenceTestBase
    {
        [Test]
        public void ByName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$foo = 'bar'",
                    "Remove-Variable foo",
                    "Get-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'foo'.", error.Exception.Message);
        }

        [Test]
        public void NameUsingNamedParametersAndAbbreviatedCommandName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$foo = 'bar'",
                    "rv -name foo",
                    "Get-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'foo'.", error.Exception.Message);
        }

        [Test]
        public void MultipleNames()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$a = 'abc'",
                    "$b = 'abc'",
                    "Remove-Variable a,b",
                    "Get-Variable a,b"
                });
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            StringAssert.Contains("Cannot find a variable", error1.Exception.Message);
            StringAssert.Contains("name 'a'.", error1.Exception.Message);
            StringAssert.Contains("Cannot find a variable", error2.Exception.Message);
            StringAssert.Contains("name 'b'.", error2.Exception.Message);
        }

        [Test]
        public void UnknownNameCausesError()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Remove-Variable unknownvariable");
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'unknownvariable'.", error.Exception.Message);
            Assert.AreEqual("VariableNotFound,Microsoft.PowerShell.Commands.RemoveVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("unknownvariable", error.TargetObject);
            Assert.IsInstanceOf<ItemNotFoundException>(error.Exception);
            Assert.AreEqual("Remove-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.ObjectNotFound, error.CategoryInfo.Category);
            Assert.AreEqual("ItemNotFoundException", error.CategoryInfo.Reason);
            Assert.AreEqual("unknownvariable", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
        }

        [Test]
        public void TryToRemoveReadOnlyVariable()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable foo 'bar' -option readonly",
                    "Remove-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            var sessionStateException = error.Exception as SessionStateUnauthorizedAccessException;

            Assert.AreEqual("foo", error.TargetObject);
            Assert.AreEqual("Cannot remove variable foo because it is constant or read-only. If the variable is read-only, try the operation again specifying the Force option.", error.Exception.Message);
            Assert.IsInstanceOf<SessionStateUnauthorizedAccessException>(error.Exception);
            Assert.AreEqual("foo", sessionStateException.ItemName);
            Assert.AreEqual(SessionStateCategory.Variable, sessionStateException.SessionStateCategory);
            Assert.AreEqual("System.Management.Automation", sessionStateException.Source);
            Assert.AreEqual("Remove-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.WriteError, error.CategoryInfo.Category);
            Assert.AreEqual("SessionStateUnauthorizedAccessException", error.CategoryInfo.Reason);
            Assert.AreEqual("foo", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
            Assert.AreEqual("VariableNotRemovable,Microsoft.PowerShell.Commands.RemoveVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void TryToRemoveConstVariable()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable foo 'bar' -option constant",
                    "Remove-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("Cannot remove variable foo because it is constant or read-only. If the variable is read-only, try the operation again specifying the Force option.", error.Exception.Message);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void TryToRemoveTwoConstantVariables()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable -name foo -option constant",
                    "Set-Variable -name bar -option constant",
                    "Remove-Variable -name foo,bar"
                });
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            Assert.AreEqual(2, ReferenceHost.GetLastRawErrorRecords().Count());
            Assert.AreEqual("Cannot remove variable foo because it is constant or read-only. If the variable is read-only, try the operation again specifying the Force option.", error1.Exception.Message);
            Assert.AreEqual("Cannot remove variable bar because it is constant or read-only. If the variable is read-only, try the operation again specifying the Force option.", error2.Exception.Message);
        }

        [Test]
        public void RemoveReadOnlyVariableUsingForce()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable foo 'bar' -option readonly",
                    "Remove-Variable foo -force",
                    "Get-Variable foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'foo'.", error.Exception.Message);
        }

        [Test]
        public void TryToRemoveConstantVariableWithForce()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "Set-Variable -name foo -option constant",
                    "Remove-Variable -name foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("Cannot remove variable foo because it is constant or read-only. If the variable is read-only, try the operation again specifying the Force option.", error.Exception.Message);
        }

        [Test]
        public void RemoveLocalScopeVariable()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$test = 'test-global'",
                    "function foo { $test = 'test-local'; Remove-Variable test -Scope local; Get-Variable test -scope local; }",
                    "foo",
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'test'.", error.Exception.Message);
        }

        [Test]
        public void RemoveGlobalScopeVariable()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$test = 'test-global'",
                    "function foo { $test = 'test-local'; Remove-Variable test -Scope global; }",
                    "foo",
                    "Get-Variable test"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'test'.", error.Exception.Message);
        }

        [Test]
        public void WildcardRemovesMultipleVariableValues()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$testaa= 'abc'",
                    "$testab = 'abc'",
                    "Remove-Variable testa*",
                    "Get-Variable testaa,testab"
                });
            });

            ErrorRecord error1 = ReferenceHost.GetLastRawErrorRecords().First();
            ErrorRecord error2 = ReferenceHost.GetLastRawErrorRecords().Last();
            StringAssert.Contains("Cannot find a variable", error1.Exception.Message);
            StringAssert.Contains("name 'testaa'.", error1.Exception.Message);
            StringAssert.Contains("Cannot find a variable", error2.Exception.Message);
            StringAssert.Contains("name 'testab'.", error2.Exception.Message);
        }

        [Test]
        [Explicit("Does not work with Pash. Global variable should not be cleared.")]
        public void NameIsWildcardAndLocalScope()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$test1 = 'test-global'",
                    "function foo { $test2 = 'test-local'; Remove-Variable test* -Scope local; Get-Variable test1,test2}",
                    "foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'test2'.", error.Exception.Message);
        }

        [Test]
        public void NameIsWildcardAndGlobalScope()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$test1 = 'test-global'",
                    "function foo { $test2 = 'test-local'; Remove-Variable test* -Scope global; Get-Variable test1,test2}",
                    "foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'test1'.", error.Exception.Message);
        }

        [Test]
        public void PrivateVariableWithWildcardNoErrorsReportedAndPrivateVariableNotRemoved()
        {
            Assert.DoesNotThrow(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name foo1 -value 'abc' -visibility private -passthru",
                    "Remove-Variable foo?"
                });
            });
        }

        [Test]
        public void UnknownVariableWithWildcardEscaped()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Remove-Variable '`?unknown`?'");
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name '`?unknown`?'.", error.Exception.Message);
            Assert.AreEqual("`?unknown`?", error.TargetObject);
            Assert.AreEqual("`?unknown`?", error.CategoryInfo.TargetName);
        }

        [Test]
        public void WildcardEscapedToRemoveVariableWithWildcardInName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name 'a?b' -value 'a'",
                    "Remove-Variable 'a`?b'",
                    "Get-Variable 'a`?b'"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'a`?b'.", error.Exception.Message);
        }

        [Test]
        public void IncludeNamesByWildcard()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$aa = 'aa'",
                    "$ba = 'ba'",
                    "Remove-Variable aa,ba -include b*",
                    "Get-Variable aa,ba"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'ba'.", error.Exception.Message);
        }

        [Test]
        public void ExcludeNamesByWildcard()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$aa = 'aa'",
                    "$ba = 'ba'",
                    "Remove-Variable aa,ba -exclude b*",
                     "Get-Variable aa,ba"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'aa'.", error.Exception.Message);
        }

        [Test]
        public void WildcardAndExcludeOneVariableName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$testaa = 'aa'",
                    "$testab = 'ab'",
                    "Remove-Variable testa* -exclude testaa",
                    "Get-Variable testaa,testab"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            StringAssert.Contains("Cannot find a variable", error.Exception.Message);
            StringAssert.Contains("name 'testab'.", error.Exception.Message);
        }
    }
}

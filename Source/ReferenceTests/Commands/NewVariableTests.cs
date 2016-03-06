// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class NewVariableTests : ReferenceTestBase
    {
        [Test]
        public void NameAndValue()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable foo 'bar'",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NameAndValueUsingNamedParameters()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable -name foo -value 'bar'",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void ValueFromPipeline()
        {
            string result = ReferenceHost.Execute(new string[] {
                "'bar' | New-Variable foo",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void Passthru()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$output = nv foo 'bar' -passthru",
                "'Name=' + $output.Name + ', Value=' + $output.Value + ', Type=' + $output.GetType().Name"
            });

            Assert.AreEqual("Name=foo, Value=bar, Type=PSVariable" + Environment.NewLine, result);
        }

        [Test]
        public void VariableDescription()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable foo 'bar' -description 'Variable Description'",
                "(Get-Variable foo).Description"
            });

            Assert.AreEqual("Variable Description" + Environment.NewLine, result);
        }

        [Test]
        public void VariableOptionsIsNoneByDefault()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable foo 'bar'",
                "(Get-Variable foo).Options.ToString()"
            });

            Assert.AreEqual("None" + Environment.NewLine, result);
        }

        [Test]
        public void ReadOnlyVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable foo 'bar' -option readonly",
                "(Get-Variable foo).Options.ToString()"
            });

            Assert.AreEqual("ReadOnly" + Environment.NewLine, result);
        }

        [Test]
        public void CannotModifyReadOnlyVariable()
        {
            var ex = Assert.Throws<SessionStateUnauthorizedAccessException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable foo 'bar' -option readonly",
                    "$foo = 'abc'"
                });
            });

            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", ex.Message);
            Assert.AreEqual("foo", ex.ItemName);
            Assert.AreEqual(SessionStateCategory.Variable, ex.SessionStateCategory);
        }

        [Test]
        public void CannotModifyConstantVariable()
        {
            var ex = Assert.Throws<SessionStateUnauthorizedAccessException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable foo 'bar' -option constant",
                    "$foo = 'abc'"
                });
            });

            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", ex.Message);
            Assert.AreEqual("foo", ex.ItemName);
            Assert.AreEqual("System.Management.Automation", ex.Source);
            Assert.AreEqual(SessionStateCategory.Variable, ex.SessionStateCategory);
            Assert.AreEqual(String.Empty, ex.ErrorRecord.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.WriteError, ex.ErrorRecord.CategoryInfo.Category);
            Assert.AreEqual("ParentContainsErrorRecordException", ex.ErrorRecord.CategoryInfo.Reason);
            Assert.AreEqual("foo", ex.ErrorRecord.CategoryInfo.TargetName);
            Assert.AreEqual("String", ex.ErrorRecord.CategoryInfo.TargetType);
            Assert.IsNull(ex.ErrorRecord.ErrorDetails);
            Assert.IsInstanceOf<ParentContainsErrorRecordException>(ex.ErrorRecord.Exception);
            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", ex.ErrorRecord.Exception.Message);
            Assert.AreEqual("VariableNotWritable", ex.ErrorRecord.FullyQualifiedErrorId);
            Assert.AreEqual("foo", ex.ErrorRecord.TargetObject);
        }

        [Test]
        public void CreateReadOnlyVariableThenTryToCreateWritableVariableWithSameName()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable foo 'bar' -option readonly",
                    "New-Variable foo 'abc'"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            var sessionStateException = error.Exception as SessionStateException;

            Assert.AreEqual("foo", error.TargetObject);
            Assert.AreEqual("A variable with name 'foo' already exists.", error.Exception.Message);
            Assert.IsInstanceOf<SessionStateException>(error.Exception);
            Assert.AreEqual("foo", sessionStateException.ItemName);
            Assert.AreEqual(SessionStateCategory.Variable, sessionStateException.SessionStateCategory);
            Assert.IsNull(sessionStateException.Source);
            Assert.AreEqual("New-Variable", error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.ResourceExists, error.CategoryInfo.Category);
            Assert.AreEqual("SessionStateException", error.CategoryInfo.Reason);
            Assert.AreEqual("foo", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
            Assert.AreEqual("VariableAlreadyExists,Microsoft.PowerShell.Commands.NewVariableCommand", error.FullyQualifiedErrorId);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void CreateVariableWithSameNameTwice()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable foo 'bar'",
                    "New-Variable foo 'abc'"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("A variable with name 'foo' already exists.", error.Exception.Message);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void CreateVariableWithSameNameTwiceUsingForce()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable foo 'bar'",
                "New-Variable foo 'abc' -force",
                "$foo"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void CreateReadOnlyVariableThenTryToCreateWritableVariableWithSameNameUsingForce()
        {
            string result = ReferenceHost.Execute(new string[] {
                "New-Variable foo 'bar' -option readonly",
                "New-Variable foo 'abc' -force",
                "$foo"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void TryToForceCreateConstVariableWithSameName()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable foo 'bar' -option constant",
                    "New-Variable foo 'abc' -force"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", error.Exception.Message);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void VisibilityIsPrivate()
        {
            var ex = Assert.Throws<SessionStateException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "nv -name foo -visibility private",
                    "$foo"
                });
            });

            ErrorRecord error = ex.ErrorRecord;
            Assert.AreEqual(0, ReferenceHost.GetLastRawErrorRecords().Count());
            Assert.AreEqual("foo", error.TargetObject);
            StringAssert.Contains("Cannot access the variable '$foo' because it is a private variable", error.Exception.Message);
            Assert.IsInstanceOf<ParentContainsErrorRecordException>(error.Exception);
            Assert.AreEqual("foo", ex.ItemName);
            Assert.AreEqual(SessionStateCategory.Variable, ex.SessionStateCategory);
            Assert.AreEqual("System.Management.Automation", ex.Source);
            Assert.AreEqual(String.Empty, error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.PermissionDenied, error.CategoryInfo.Category);
            Assert.AreEqual("ParentContainsErrorRecordException", error.CategoryInfo.Reason);
            Assert.AreEqual("foo", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
            Assert.AreEqual("VariableIsPrivate", error.FullyQualifiedErrorId);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void DefaultVisibilityIsPublic()
        {
            string result = ReferenceHost.Execute(new string[] {
                "nv -name foo",
                "(Get-Variable foo).Visibility.ToString()"
            });

            Assert.AreEqual("Public" + Environment.NewLine, result);
        }

        [Test]
        public void VisibilityIsPrivatePassThru()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = New-Variable -name foo -passthru -visibility private",
                "$a.Visibility.ToString()"
            });

            Assert.AreEqual("Private" + Environment.NewLine, result);
        }

        [Test]
        public void CannotSetPrivateVariableValue()
        {
            var ex = Assert.Throws<SessionStateException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable -name foo -visibility private",
                    "$foo = 'abc'"
                });
            });

            ErrorRecord error = ex.ErrorRecord;
            Assert.AreEqual(0, ReferenceHost.GetLastRawErrorRecords().Count());
            Assert.AreEqual("foo", error.TargetObject);
            StringAssert.Contains("Cannot access the variable '$foo' because it is a private variable", error.Exception.Message);
            Assert.IsInstanceOf<ParentContainsErrorRecordException>(error.Exception);
            Assert.AreEqual("foo", ex.ItemName);
            Assert.AreEqual(SessionStateCategory.Variable, ex.SessionStateCategory);
            Assert.AreEqual("System.Management.Automation", ex.Source);
            Assert.AreEqual(String.Empty, error.CategoryInfo.Activity);
            Assert.AreEqual(ErrorCategory.PermissionDenied, error.CategoryInfo.Category);
            Assert.AreEqual("ParentContainsErrorRecordException", error.CategoryInfo.Reason);
            Assert.AreEqual("foo", error.CategoryInfo.TargetName);
            Assert.AreEqual("String", error.CategoryInfo.TargetType);
            Assert.AreEqual("VariableIsPrivate", error.FullyQualifiedErrorId);
            Assert.AreEqual("foo", error.TargetObject);
        }

        [Test]
        public void NewVariableInsideFunctionHasLocalScopeByDefault()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function foo { New-Variable test 'test-local' -scope local }",
                "foo",
                "$test"
            });

            Assert.AreEqual(Environment.NewLine, result);
        }

        [Test]
        public void FunctionLocalScopeVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function foo { New-Variable test 'test-local' -scope local }",
                "foo",
                "$test"
            });

            Assert.AreEqual(Environment.NewLine, result);
        }

        [Test]
        public void FunctionGlobalScopeVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function foo {  New-Variable test 'test-global' -scope global }",
                "foo",
                "$test"
            });

            Assert.AreEqual("test-global" + Environment.NewLine, result);
        }

        [Test]
        public void CreateGlobalScopeVariableInFunctionWhenVariableAlreadyDefined()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "$a = 'bar'",
                    "function foo { New-Variable a 'abc' -scope global }",
                    "foo"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("A variable with name 'a' already exists.", error.Exception.Message);
            Assert.AreEqual("a", error.TargetObject);
        }

        [Test]
        public void CreateLocalScopeVariableInFunctionWhenVariableAlreadyDefinedGlobally()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = 'bar'",
                "function foo { New-Variable a 'abc' -scope local; $a }",
                "foo"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void CreateVariableInFunctionWhenVariableAlreadyDefinedGlobally()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = 'bar'",
                "function foo { New-Variable a 'abc'; $a }",
                "foo"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }
    }
}

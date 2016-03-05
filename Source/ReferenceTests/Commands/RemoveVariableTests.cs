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
            Assert.AreEqual("Cannot find a variable with name 'foo'.", error.Exception.Message);
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
            Assert.AreEqual("Cannot find a variable with name 'foo'.", error.Exception.Message);
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
            Assert.AreEqual("Cannot find a variable with name 'a'.", error1.Exception.Message);
            Assert.AreEqual("Cannot find a variable with name 'b'.", error2.Exception.Message);
        }

        [Test]
        public void UnknownNameCausesError()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute("Remove-Variable unknownvariable");
            });

            //ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            //Assert.AreEqual("Cannot find a variable with name 'unknownvariable'.", error.Exception.Message);
            //Assert.AreEqual("VariableNotFound,Microsoft.PowerShell.Commands.RemoveVariableCommand", error.FullyQualifiedErrorId);
            //Assert.AreEqual("unknownvariable", error.TargetObject);
            //Assert.IsInstanceOf<ItemNotFoundException>(error.Exception);
            //Assert.AreEqual("Remove-Variable", error.CategoryInfo.Activity);
            //Assert.AreEqual(ErrorCategory.ObjectNotFound, error.CategoryInfo.Category);
            //Assert.AreEqual("ItemNotFoundException", error.CategoryInfo.Reason);
            //Assert.AreEqual("unknownvariable", error.CategoryInfo.TargetName);
            //Assert.AreEqual("String", error.CategoryInfo.TargetType);
        }
    }
}

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
        [Explicit("Not supported in Pash")]
        public void CreateReadOnlyVariableThenTryToCreateWritableVariableWithSameName()
        {
            Assert.Throws<ExecutionWithErrorsException>(delegate
            {
                ReferenceHost.Execute(new string[] {
                    "New-Variable foo 'bar' -option readonly",
                    "New-Variable foo 'abc'"
                });
            });

            ErrorRecord error = ReferenceHost.GetLastRawErrorRecords().Single();
            Assert.AreEqual("foo", error.TargetObject);
        }
    }
}

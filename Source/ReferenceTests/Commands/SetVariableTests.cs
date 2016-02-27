// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class SetVariableTests : ReferenceTestBase
    {
        [Test]
        public void NameAndValue()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable foo 'bar'",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NameAndValueUsingNamedParameters()
        {
            string result = ReferenceHost.Execute(new string[] {
                "set -name foo -value 'bar'",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void ValueFromPipeline()
        {
            string result = ReferenceHost.Execute(new string[] {
                "'bar' | sv foo",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void NameAndValueForVariableThatAlreadyExists()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'abc'",
                "Set-Variable foo 'bar'",
                "$foo"
            });

            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void MultipleNamesSingleValue()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable a,b 'bar'",
                "$a + \", \" + $b"
            });

            Assert.AreEqual("bar, bar" + Environment.NewLine, result);
        }

        [Test]
        public void NoValue()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable foo",
                "$foo -eq $null"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void SingleNameMultipleValues()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable a 'foo','bar'",
                "$a[0] + \", \" + $a[1]"
            });

            Assert.AreEqual("foo, bar" + Environment.NewLine, result);
        }

        [Test]
        public void SingleNameMultipleValuesOnPipeline()
        {
            string result = ReferenceHost.Execute(new string[] {
                "'foo','bar' | Set-Variable a",
                "$a[0] + \", \" + $a[1] + \" - Type=\" + $a.GetType().Name"
            });

            Assert.AreEqual("foo, bar - Type=Object[]" + Environment.NewLine, result);
        }

        [Test]
        public void Passthru()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$output = Set-Variable foo 'bar' -passthru",
                "'Name=' + $output.Name + ', Value=' + $output.Value + ', Type=' + $output.GetType().Name"
            });

            Assert.AreEqual("Name=foo, Value=bar, Type=PSVariable" + Environment.NewLine, result);
        }

        [Test]
        public void PassthruTwoVariablesCreated()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$v= Set-Variable foo,bar 'abc' -passthru",
                "'Names=' + $v[0].Name + ',' + $v[1].Name + ' Values=' + $v[0].Value + ',' + $v[1].Value + ' Type=' + $v.GetType().Name + ' ' + $v[0].GetType().Name"
            });

            Assert.AreEqual("Names=foo,bar Values=abc,abc Type=Object[] PSVariable" + Environment.NewLine, result);
        }

        [Test]
        public void VariableDescription()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable foo 'bar' -description 'Variable Description'",
                "(Get-Variable foo).Description"
            });

            Assert.AreEqual("Variable Description" + Environment.NewLine, result);
        }

        [Test]
        public void DefaultVariableDescriptionIsEmptyString()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable foo 'bar'",
                "(Get-Variable foo).Description"
            });

            Assert.AreEqual(String.Empty + Environment.NewLine, result);
        }

        [Test]
        public void VariableDescriptionForExistingVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "Set-Variable foo 'bar' -description 'desc 1'",
                "Set-Variable foo 'bar' -description 'Variable Description'",
                "(Get-Variable foo).Description"
            });

            Assert.AreEqual("Variable Description" + Environment.NewLine, result);
        }
    }
}

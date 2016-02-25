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
        [Explicit("Does not work with Pash")]
        public void SingleNameMultipleValuesOnPipeline()
        {
            string result = ReferenceHost.Execute(new string[] {
                "'foo','bar' | Set-Variable a",
                "$a[0] + \", \" + $a[1]"
            });

            Assert.AreEqual("foo, bar" + Environment.NewLine, result);
        }
    }
}

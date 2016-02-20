// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class VariableTests : ReferenceTestBase
    {
        [TestCase("$x")] // normal
        [TestCase("$global:x")] // scope qualified
        [TestCase("$foo:x")] // drive qualified
        [TestCase("$:x", Ignore = true)] // PS isn't too harsh when it comes to colons in the name. Pash needs to relax
        public void UnknownVariableIsSimplyNull(string name)
        {
            var res = ReferenceHost.RawExecute(name);
            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0], Is.Null);
        }

        [Test]
        public void SettingAVariableOnlyModifiesTheObject()
        {
            var cmd = "$x = 1; $var = Get-Variable x; $var.Value; $x = 2; $var.Value";
            ExecuteAndCompareTypedResult(cmd, 1, 2);
        }

        [Test]
        public void SetVariableValueViaObject()
        {
            var cmd = "$x = 1; $var = Get-Variable x; $var.Value = 2; $x";
            ExecuteAndCompareTypedResult(cmd, 2);
        }

        [Test]
        public void HostVariableIsAvailable()
        {
            string result = ReferenceHost.Execute("$host -eq $null");

            Assert.AreEqual("False" + Environment.NewLine, result);
        }

        [Test]
        public void HostVariableIsConstant()
        {
            string result = ReferenceHost.Execute("(Get-Variable host).Options.ToString()");

            Assert.AreEqual("Constant, AllScope" + Environment.NewLine, result);
        }

        [Test]
        public void CannotModifyHostVariableValue()
        {
            Assert.Throws<SessionStateUnauthorizedAccessException>(delegate
            {
                ReferenceHost.Execute("$host = 'abc'");
            });
        }

        [Test]
        public void ErrorVariableIsConstant()
        {
            string result = ReferenceHost.Execute("(Get-Variable error).Options.ToString()");

            Assert.AreEqual("Constant" + Environment.NewLine, result);
        }

        [Test]
        public void TrueVariableIsConstant()
        {
            string result = ReferenceHost.Execute("(Get-Variable true).Options.ToString()");

            Assert.AreEqual("Constant, AllScope" + Environment.NewLine, result);
        }

        [Test]
        public void FalseVariableIsConstant()
        {
            string result = ReferenceHost.Execute("(Get-Variable false).Options.ToString()");

            Assert.AreEqual("Constant, AllScope" + Environment.NewLine, result);
        }

        [Test]
        public void QuestionMarkVariableIsReadOnly()
        {
            string result = ReferenceHost.Execute("(Get-Variable '?' | ? { $_.Name -eq '?' }).Options.ToString()");

            Assert.AreEqual("ReadOnly, AllScope" + Environment.NewLine, result);
        }

        [Test]
        public void NullVariableHasNoOptionsSet()
        {
            string result = ReferenceHost.Execute("(Get-Variable null).Options.ToString()");

            Assert.AreEqual("None" + Environment.NewLine, result);
        }

        [Test]
        public void NullVariableCanBeSetButValueIsUnchanged()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$null = 'abc'",
                "$null"
            });

            Assert.AreEqual(Environment.NewLine, result);
        }

        [Test]
        public void NullVariableOptionsCanBeSetButValueIsUnchanged()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = get-variable null",
                "$a.Options = 'AllScope'",
                "$a.Options.ToString()"
            });

            Assert.AreEqual("None" + Environment.NewLine, result);
        }

        /// <summary>
        /// Not sure why PowerShell allows this.
        /// </summary>
        [Test]
        [Explicit("PSVariable does not have a Visibility property in Pash.")]
        public void NullVariableVisibilityCanBeSetAndValueIsChanged()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = get-variable null",
                "$a.Visibility = 'Private'",
                "$a.Visibility.ToString()"
            });

            Assert.AreEqual("Private" + Environment.NewLine, result);
        }
    }
}


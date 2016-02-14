// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.API
{
    [TestFixture]
    public class RunspaceInitialVariables : ReferenceTestBase
    {
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
    }
}

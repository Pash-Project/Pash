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
    }
}

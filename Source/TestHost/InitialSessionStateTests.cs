// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using NUnit.Framework;
using System.Management.Automation.Runspaces;

namespace TestHost
{
    [TestFixture]
    public class InitialSessionStateTests
    {
        [Test]
        public void VariableDefinedInInitialSessionStateCanBeUsedInStatement()
        {
            string variableValue = "testVariableValue";
            var variableEntry = new SessionStateVariableEntry("testVariable", variableValue, "description");
            InitialSessionState sessionState = InitialSessionState.Create();
            sessionState.Variables.Add(variableEntry);
            TestHost.InitialSessionState = sessionState;

            string output = TestHost.Execute("$testVariable");

            Assert.AreEqual(variableValue + Environment.NewLine, output);
        }
    }
}

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

        [Test]
        public void ImportPSModuleByFileNameAllowsCmdletInModuleToBeUsed()
        {
            InitialSessionState sessionState = InitialSessionState.Create();
            string fileName = typeof(InitialSessionStateTests).Assembly.Location;
            sessionState.ImportPSModule(new string[] { fileName });
            TestHost.InitialSessionState = sessionState;

            string output = TestHost.Execute("Invoke-Test -Parameter ParameterValue");

            Assert.AreEqual("Parameter='ParameterValue'" + Environment.NewLine, output);
        }
    }
}

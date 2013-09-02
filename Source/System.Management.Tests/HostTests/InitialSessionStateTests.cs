// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using NUnit.Framework;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;

namespace System.Management.Tests.HostTests
{
    [TestFixture]
    public class InitialSessionStateTests
    {
        [Test]
        public void DefaultInitialStateHasNoModules()
        {
            InitialSessionState sessionState = InitialSessionState.CreateDefault();

            Assert.AreEqual(0, sessionState.Modules.Count);
        }

        [Test]
        public void ImportPSModuleAddsModule()
        {
            InitialSessionState sessionState = InitialSessionState.CreateDefault();
            string fileName = @"c:\MyCmdlets\MyCmdlet.dll";

            sessionState.ImportPSModule(new string[] { fileName });

            ModuleSpecification moduleSpec = sessionState.Modules[0];
            Assert.AreEqual(fileName, moduleSpec.Name);
        }

        [Test]
        public void AddVariableToSessionState()
        {
            InitialSessionState sessionState = InitialSessionState.CreateDefault();
            var variableEntry = new SessionStateVariableEntry("TestVariable", new object(), "description");

            sessionState.Variables.Add(variableEntry);

            Assert.IsTrue(sessionState.Variables.Contains(variableEntry));
        }
    }
}

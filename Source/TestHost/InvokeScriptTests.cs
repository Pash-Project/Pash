// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using NUnit.Framework;
using System.Management.Automation.Runspaces;
using System.IO;

namespace TestHost
{
    [TestFixture]
    public class InvokeScriptTests
    {
        private void ImportTestInvokeScriptCmdlet()
        {
            InitialSessionState sessionState = InitialSessionState.Create();
            string fileName = typeof(InvokeScriptTests).Assembly.Location;
            sessionState.ImportPSModule(new string[] { fileName });
            TestHost.InitialSessionState = sessionState;
        }

        [TearDown]
        public void ResetInitialSessionState()
        {
            //necessarry as TestHost is (unfortunately) used in a static way
            TestHost.InitialSessionState = null;
        }

        [Test]
        public void ScriptCanBeInvokedByPSCmdletInvokeCommand()
        {
            ImportTestInvokeScriptCmdlet();
            string statement = string.Format("Test-InvokeScript -Script 'Write-Host \"Script output\"'");

            string output = TestHost.Execute(statement);

            Assert.AreEqual("Script output" + Environment.NewLine, output);
        }
    }
}

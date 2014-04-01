// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using TestPSSnapIn;

namespace ReferenceTests
{
    [TestFixture]
    public class PipelineExecutionTets : ReferenceTestBase
    {

        [SetUp]
        public void ImportTestInvokeScriptCmdlet()
        {
            ReferenceHost.ImportModules(new string[] {  typeof(TestCommand).Assembly.Location });
        }

        [TearDown]
        public void ResetInitialSessionState()
        {
            ReferenceHost.ImportModules(null);
        }

        [Test]
        public void AllPhasesAreRunAsSingleCommand()
        {
            var cmd = CmdletName(typeof(TestCmdletPhasesCommand));
            var res = ReferenceHost.Execute(cmd);
            var expected = OutputString(new string[] { "Begin", "Process", "End" });
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NullAsInputOfFirstCommandDoesInvokeProcessRecords()
        {
            var cmd = "$null | " + CmdletName(typeof(TestCmdletPhasesCommand));
            var res = ReferenceHost.Execute(cmd);
            var expected = OutputString(new string[] { "Begin", "Process", "End" });
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NoPipelineInputToSecondCommandInPipelineSkipsProcessRecords()
        {
            var cmd = CmdletName(typeof(TestDummyCommand)) + " | " + CmdletName(typeof(TestCmdletPhasesCommand));
            var res = ReferenceHost.Execute(cmd);
            var expected = OutputString(new string[] { "Begin", "End" });
            Assert.AreEqual(expected, res);
        }
    }
}


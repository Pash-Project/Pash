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
        [Test]
        public void AllPhasesAreRunAsSingleCommand()
        {
            var cmd = CmdletName(typeof(TestCmdletPhasesCommand));
            var res = ReferenceHost.Execute(cmd);
            var expected = NewlineJoin("Begin", "Process", "End");
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NullAsInputOfFirstCommandDoesInvokeProcessRecords()
        {
            var cmd = "$null | " + CmdletName(typeof(TestCmdletPhasesCommand));
            var res = ReferenceHost.Execute(cmd);
            var expected = NewlineJoin("Begin", "Process", "End");
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void NoPipelineInputToSecondCommandInPipelineSkipsProcessRecords()
        {
            var cmd = CmdletName(typeof(TestDummyCommand)) + " | " + CmdletName(typeof(TestCmdletPhasesCommand));
            var res = ReferenceHost.Execute(cmd);
            var expected = NewlineJoin("Begin", "End");
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void MultipleObjectsAsPipelineCommandScriptsReturnLastObject()
        {
            var expected = NewlineJoin("bar");
            var result = ReferenceHost.Execute(new [] { "'foo'", "'bar'" });
            Assert.AreEqual(expected, result);
        }

    }
}


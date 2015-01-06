// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using TestPSSnapIn;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class PipelineExecutionTests : ReferenceTestBaseWithTestModule
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

        [Test]
        public void HashtableIsNotEnumerated()
        {
            var expected = NewlineJoin(typeof(System.Collections.Hashtable).FullName);
            var result = ReferenceHost.Execute("@{foo='bar';bar=2} | foreach-object { $_.GetType().FullName }");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GeneratedEnumerableIsEnumeratedOnWriting()
        {
            var expected = NewlineJoin("10", "11", "12");
            var result = ReferenceHost.Execute(CmdletName(typeof(TestWriteEnumerableToPipelineCommand)) + " 10 12");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CmdletCanWriteNullToPipeline()
        {
            var cmd = CmdletName(typeof(TestWriteNullCommand));
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(""), result);
        }

        [Test]
        public void ProcessRecordWorksWithNulls()
        {
            var cmd = "@($null, $null, $null, $null) | " + CmdletName(typeof(TestCountProcessRecordCommand));
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin("4"), result);
        }

    }
}


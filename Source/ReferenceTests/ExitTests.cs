// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class ExitTests : ReferenceTestBase
    {
        [Test]
        public void ExitWorks()
        {
            Assert.DoesNotThrow(
                delegate() { ReferenceHost.Execute("exit 4"); }
            );
        }

        [Test]
        public void ExitInterruptsCommands()
        {
            var result = ReferenceHost.Execute("'foo'; exit 4; 'bar'");
            Assert.AreEqual(NewlineJoin("foo"), result);
        }

        [Test]
        public void ExitInScriptOnlyEndsScript()
        {
            var scriptname = CreateScript(NewlineJoin(
                "'foo'",
                "exit 4",
                "'bar'"
            ));
            var command = NewlineJoin(
                String.Format(". '{0}'", scriptname),
                "$LastExitCode"
            );
            var expected = NewlineJoin("foo", "4");
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [TestCase("0", "'some string'")]
        [TestCase("8", "4 + 4")]
        [TestCase("3", "& { 3 }")]
        public void ExpressionsAreValidExitCodes(string expectedExitCode, string exitExpression)
        {
            var scriptname = CreateScript(NewlineJoin("exit " + exitExpression));
            var command = NewlineJoin(
                String.Format(". '{0}'", scriptname),
                "$LastExitCode"
            );
            var expected = NewlineJoin(expectedExitCode);
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [TestCase("False", "-1", true)]
        [TestCase("True", "0", true)]
        [TestCase("False", "92", true)]
        [TestCase("True", "'some string'", false)]
        public void ExitCodeDefinesSuccess(string success, string exitCode, bool exitCodeValid)
        {
            var scriptname = CreateScript(NewlineJoin("exit " + exitCode));
            var command = NewlineJoin(
                String.Format(". '{0}'", scriptname),
                "$?",
                "$LastExitCode"
            );
            var expected = NewlineJoin(success, exitCodeValid ? exitCode : "0");
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ScriptWithoutExitIsSuccessfull()
        {
            var scriptname = CreateScript(NewlineJoin("'foo'"));
            var command = NewlineJoin(
                String.Format(". '{0}'", scriptname),
                "$?",
                "$LastExitCode"
            );
            var expected = NewlineJoin("foo", "True", "");
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test, Explicit("Pipeline on-demand processing doesn't work")]
        public void ExitInScriptBlockExitsCompletely()
        {
            var command = NewlineJoin(
                "'foo'",
                "& { 'bar'; exit; 'baz'; }",
                "'blub'"
            );
            var expected = NewlineJoin("foo", "bar");
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test, Explicit("Pipeline on-demand processing doesn't work")]
        public void ExitInFunctionExitsCompletely()
        {
            var command = NewlineJoin(
                "function testfun { 'bar'; exit; 'baz'; }",
                "'foo'",
                "testfun",
                "'blub'"
            );
            var expected = NewlineJoin("foo", "bar");
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ExitCodeIsProcessExitCode()
        {
            var process = CreatePowershellOrPashProcess("exit 5");
            Assert.True(process.Start());
            FinishProcess(process);
            Assert.AreEqual(5, process.ExitCode);
        }

        [Test, Explicit("Pipeline on-demand processing doesn't work")]
        public void ProcessExistsAndPrintsFirst()
        {
            var process = CreatePowershellOrPashProcess("'foo'; & { exit 5; 'bar'; }; 'baz'");
            Assert.True(process.Start());
            var expectedOutput = NewlineJoin("foo");
            var output = FinishProcess(process);
            Assert.AreEqual(5, process.ExitCode);
            Assert.AreEqual(expectedOutput, output);
        }
    }
}


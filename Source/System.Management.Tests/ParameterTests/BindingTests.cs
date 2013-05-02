// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using NUnit.Framework;

namespace System.Management.Tests.ParameterTests
{
    [TestFixture]
    public class BindingTests
    {
        private CmdletInfo info = null;

        [SetUp]
        public void LoadCmdInfo()
        {
            info = TestParameterCommand.CreateCmdletInfo();
        }

        [Test]
        public void BindingField()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("Name", "John");

            cmdProc.BindArguments(null);

            Assert.AreEqual("John", cmdlet.Name);
        }

        [Test]
        public void BindingParameter()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("InputObject", 10);

            cmdProc.BindArguments(null);

            Assert.AreEqual("10", cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingNonSwitch()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("Name", null);
            cmdProc.AddParameter(null, "John");

            cmdProc.BindArguments(null);

            Assert.AreEqual("John", cmdlet.Name);
            Assert.IsFalse(cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationAllSet()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("Name", null);
            cmdProc.AddParameter(null, "John");
            cmdProc.AddParameter("Recurse", null);

            cmdProc.BindArguments(null);

            Assert.AreEqual("John", cmdlet.Name);
            Assert.IsTrue(cmdlet.Recurse.ToBool());
        }

        [Test, Explicit]
        public void BindingCombinationNonDefaultSet()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("Name", null);
            cmdProc.AddParameter(null, "John");
            cmdProc.AddParameter("FilePath", null);
            cmdProc.AddParameter(null, "a path");
            cmdProc.AddParameter("Recurse", null);

            cmdProc.BindArguments(null);

            Assert.AreEqual("John", cmdlet.Name);
            Assert.AreEqual("a path", cmdlet.FilePath);
            Assert.IsTrue(cmdlet.Recurse.ToBool());
        }

        [Test, Explicit]
        public void BindingParameterSetSelectionSingle()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("FilePath", null);
            cmdProc.AddParameter(null, "a path");

            cmdProc.BindArguments(null);

            Assert.AreEqual("File", cmdlet.ParameterSetName);
        }

        [Test, Explicit]
        public void BindingParameterSetSelectionSingleAlias()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("PSPath", null);
            cmdProc.AddParameter(null, "a path");

            cmdProc.BindArguments(null);

            Assert.AreEqual("File", cmdlet.ParameterSetName);
        }

        [Test, Explicit]
        public void BindingParameterSetSelectionDoubleShouldFail()
        {
            CommandProcessor cmdProc = new CommandProcessor(info);
            TestParameterCommand cmdlet = new TestParameterCommand();
            cmdProc.Command = cmdlet;

            cmdProc.AddParameter("Variable", null);
            cmdProc.AddParameter(null, "test");
            cmdProc.AddParameter("FilePath", null);
            cmdProc.AddParameter(null, "a path");

            Assert.Throws(typeof(Exception), delegate()
            {
                cmdProc.BindArguments(null);
            });
        }
    }
}

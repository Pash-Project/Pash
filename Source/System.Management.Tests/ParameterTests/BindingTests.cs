// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using NUnit.Framework;
using System.Management.Automation.Runspaces;

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
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("Name", "John");

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("John", cmdlet.Name);
        }

        [Test]
        public void BindingFieldAlias()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("fn", "John");

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("John", cmdlet.Name);
        }

        [Test]
        public void BindingParameter()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("InputObject", 10);

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("10", cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingParameterAlias()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("Path", "a path");

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("a path", cmdlet.FilePath.ToString());
        }

        [Test]
        public void BindingAmbiguous()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("i", 10);

            Assert.Throws(typeof(ArgumentException), delegate() {
                binder.BindCommandLineArguments(parameters);
            });
        }

        [Test]
        public void BindingNonSwitch()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("Name", null);
            parameters.Add(null, "John");

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("John", cmdlet.Name);
            Assert.IsFalse(cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationAllSet()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("Name", null);
            parameters.Add(null, "John");
            parameters.Add("Recurse", null);

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("John", cmdlet.Name);
            Assert.IsTrue(cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationNonDefaultSet()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("Variable", "a");
            parameters.Add("Recurse", null);

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("a", cmdlet.Variable);
            Assert.IsTrue(cmdlet.Recurse.ToBool());
        }

        [Test, Explicit]
        public void BindingParameterSetSelectionSingle()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("FilePath", null);
            parameters.Add(null, "a path");

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("File", cmdlet.ParameterSetName);
        }

        [Test, Explicit]
        public void BindingParameterSetSelectionSingleAlias()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("PSPath", null);
            parameters.Add(null, "a path");

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual("File", cmdlet.ParameterSetName);
        }

        [Test, Explicit("This is currently does pass, but not for the right reason (missing parameter set selection logic)")]
        public void BindingParameterSetSelectionDoubleShouldFail()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            parameters.Add("Variable", null);
            parameters.Add(null, "test");
            parameters.Add("FilePath", null);
            parameters.Add(null, "a path");

            Assert.Throws(typeof(Exception), delegate()
            {
                    binder.BindCommandLineArguments(parameters);
            });
        }

        [Test]
        public void BindingParameterIntToIntArrayConversion()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            double pi = 3.14159;
            parameters.Add("FavoriteNumbers", pi);

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual(cmdlet.FavoriteNumbers, new double[] { pi });
        }

        [Test]
        public void BindingParameterAnyObjectToSpecificArrayConversion()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            var exception = new RuntimeException("foo");
            parameters.Add("FavoriteExceptions", exception);

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual(new Exception[] { exception }, cmdlet.FavoriteExceptions);
        }

        public void BindingParameterArrayElementConversion()
        {
            TestParameterCommand cmdlet = new TestParameterCommand();
            CmdletArgumentBinder binder = new CmdletArgumentBinder(info, cmdlet);
            CommandParameterCollection parameters = new CommandParameterCollection();

            int two = 2;
            parameters.Add("FavoriteNumbers", two);

            binder.BindCommandLineArguments(parameters);

            Assert.AreEqual(cmdlet.FavoriteNumbers, new double[] { (double) two });
        }
    }
}

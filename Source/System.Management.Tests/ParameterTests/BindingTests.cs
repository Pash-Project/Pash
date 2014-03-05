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
        private CmdletInfo _info = null;
        private CommandParameterCollection _parameters = null;
        private CmdletArgumentBinder _binder = null;
        private TestParameterCommand _cmdlet = null;

        [SetUp]
        public void LoadCmdInfo()
        {
            _info = TestParameterCommand.CreateCmdletInfo();
            _cmdlet = new TestParameterCommand();
            _binder = new CmdletArgumentBinder(_info, _cmdlet);
            _parameters = new CommandParameterCollection();
        }

        [Test]
        public void BindingField()
        {
            _parameters.Add("Name", "John");

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
        }

        [Test]
        public void BindingFieldAlias()
        {
            _parameters.Add("fn", "John");

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
        }

        [Test]
        public void BindingParameter()
        {
            _parameters.Add("InputObject", 10);

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("10", _cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingParameterAlias()
        {
            _parameters.Add("Path", "a path");

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("a path", _cmdlet.FilePath.ToString());
        }

        [Test]
        public void BindingAmbiguous()
        {
            _parameters.Add("i", 10);

            Assert.Throws(typeof(ArgumentException), delegate() {
                _binder.BindCommandLineArguments(_parameters);
            });
        }

        [Test]
        public void BindingNonSwitch()
        {
            _parameters.Add("Name", null);
            _parameters.Add(null, "John");

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
            Assert.IsFalse(_cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationAllSet()
        {
            _parameters.Add("Name", null);
            _parameters.Add(null, "John");
            _parameters.Add("Recurse", null);

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
            Assert.IsTrue(_cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationNonDefaultSet()
        {
            _parameters.Add("Variable", "a");
            _parameters.Add("Recurse", null);

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("a", _cmdlet.Variable);
            Assert.IsTrue(_cmdlet.Recurse.ToBool());
        }

        [Test, Explicit]
        public void Binding_parametersetSelectionSingle()
        {
            _parameters.Add("FilePath", null);
            _parameters.Add(null, "a path");

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test, Explicit]
        public void Binding_parametersetSelectionSingleAlias()
        {
            _parameters.Add("PSPath", null);
            _parameters.Add(null, "a path");

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test, Explicit("This is currently does pass, but not for the right reason (missing parameter set selection logic)")]
        public void Binding_parametersetSelectionDoubleShouldFail()
        {
            _parameters.Add("Variable", null);
            _parameters.Add(null, "test");
            _parameters.Add("FilePath", null);
            _parameters.Add(null, "a path");

            Assert.Throws(typeof(Exception), delegate()
            {
                    _binder.BindCommandLineArguments(_parameters);
            });
        }

        [Test]
        public void BindingParameterIntToIntArrayConversion()
        {
            double pi = 3.14159;
            _parameters.Add("FavoriteNumbers", pi);

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual(new double[] { pi }, _cmdlet.FavoriteNumbers);
        }

        [Test]
        public void BindingParameterAnyObjectToSpecificArrayConversion()
        {
            var exception = new RuntimeException("foo");
            _parameters.Add("FavoriteExceptions", exception);

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual(new Exception[] { exception }, _cmdlet.FavoriteExceptions);
        }

        public void BindingParameterArrayElementConversion()
        {
            int two = 2;
            _parameters.Add("FavoriteNumbers", two);

            _binder.BindCommandLineArguments(_parameters);

            Assert.AreEqual(new double[] { (double) two }, _cmdlet.FavoriteNumbers);
        }
    }
}

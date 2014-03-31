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
        private CmdletParameterBinder _binder = null;
        private TestParameterCommand _cmdlet = null;

        [SetUp]
        public void LoadCmdInfo()
        {
            _info = TestParameterCommand.CreateCmdletInfo();
            _cmdlet = new TestParameterCommand();
            _binder = new CmdletParameterBinder(_info, _cmdlet);
            _parameters = new CommandParameterCollection();
        }

        [Test]
        public void BindingField()
        {
            _parameters.Add("Name", "John");
            _parameters.Add("Variable", "foo"); // mandatory

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
        }

        [Test]
        public void BindingFieldAlias()
        {
            _parameters.Add("fn", "John");
            _parameters.Add("Variable", "foo"); // mandatory

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
        }

        [Test]
        public void BindingParameter()
        {
            _parameters.Add("InputObject", 10);
            _parameters.Add("Variable", "foo"); // mandatory

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("10", _cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingParameterByPipeline()
        {
            _parameters.Add("Variable", "foo"); // mandatory

            _binder.BindCommandLineParameters(_parameters);
            _binder.BindPipelineParameters(10, true);

            Assert.AreEqual("10", _cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingParameterAlias()
        {
            _parameters.Add("Path", "a path");

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("a path", _cmdlet.FilePath.ToString());
        }

        [Test]
        public void LookupOfAmbiguousParameterFails()
        {
            _parameters.Add("i", 10);

            Assert.Throws(typeof(ParameterBindingException), delegate() {
                _binder.BindCommandLineParameters(_parameters);
            });
        }

        [Test]
        public void BindingNonSwitch()
        {
            _parameters.Add("Name", "John");
            _parameters.Add("Variable", "foo"); // mandatory

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("John", _cmdlet.Name);
            Assert.IsFalse(_cmdlet.Recurse.ToBool());
        }

        [TestCase("Variable")]
        [TestCase("FilePath")]
        public void BindingCombinationAllSet(string mandatory)
        {
            _parameters.Add("Name", "John");
            _parameters.Add("Recurse", null);
            _parameters.Add(mandatory, "foo"); // chooses the parameter set

            _binder.BindCommandLineParameters(_parameters);

            // as the values are in both sets, it should be always set
            Assert.AreEqual("John", _cmdlet.Name);
            Assert.IsTrue(_cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationNonDefaultSet()
        {
            _parameters.Add("Variable", "a");
            _parameters.Add("Recurse", null);

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("a", _cmdlet.Variable);
            Assert.IsTrue(_cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingParameterSetSelectionSingle()
        {
            _parameters.Add("FilePath", "a path");

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test]
        public void BindingParameterSetSelectionSingleAlias()
        {
            _parameters.Add("PSPath", "a path");

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test]
        public void BindingParameterSetSelectionDoubleShouldFail()
        {
            _parameters.Add("Variable", "test");
            _parameters.Add("FilePath", "a path");

            Assert.Throws(typeof(ParameterBindingException), delegate()
            {
                    _binder.BindCommandLineParameters(_parameters);
            });
        }

        // TODO: refine exception types
        // general stuff
        [Test]
        public void BindingParameterTwiceShouldFail()
        {
            _parameters.Add("Variable", "foo");
            _parameters.Add("Variable", "bar");
            Assert.Throws(typeof(ParameterBindingException), delegate()
            {
                _binder.BindCommandLineParameters(_parameters);
            });
        }

        // TODO: Binding pipeline parameters:
        // test: bind input object  w/o coercing
        // test, explicit: bind input object by property name, before casting parent
        // test, explicit: bind input object with coercing, because property cannot be taken w/o coercing
        // test, explicit: bind input object by property name with coercing
        // test: wrong object should cause error

        // Binding by position
        [Test]
        public void BindingParameterByPositionSelectsDefaultParameterSetIncomplete()
        {
            var name = "foo";
            _parameters.Add(null, name);

            var ex = Assert.Throws(typeof(ParameterBindingException), delegate()
            {
                _binder.BindCommandLineParameters(_parameters);
            });
            StringAssert.Contains("FilePath", ex.Message); // should complain that no "FilePath" was provided
        }

        [Test]
        public void BindingParameterByPositionSelectsDefaultParameterSet()
        {
            var name = "foo";
            var path = "a path";
            _parameters.Add(null, name);
            _parameters.Add(null, path);

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual(path, _cmdlet.FilePath);
            Assert.AreEqual(name, _cmdlet.Name);
            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test]
        public void BindingParameterByPositionAfterSetSelection()
        {
            var varname = "foo";
            _parameters.Add("ConstVar", null); // switch parameter, should select "Variable" set
            _parameters.Add(null, varname);

            _binder.BindCommandLineParameters(_parameters);

            Assert.IsTrue(_cmdlet.ConstVar.ToBool());
            Assert.AreEqual(varname, _cmdlet.Variable);
            Assert.AreEqual("Variable", _cmdlet.ParameterSetName);
        }

        // TODO: automatic input
        // test: don't provide any parameters. It should get FilePath by stdin. set should be "File"
        // test: set ConstVar, then bind. It should get Variable by stdin. set should be "Variable"

        // conversion/coercing
        [Test]
        public void BindingParameterIntToIntArrayConversion()
        {
            double pi = 3.14159;
            _parameters.Add("FavoriteNumbers", pi);

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual(new double[] { pi }, _cmdlet.FavoriteNumbers);
        }

        [Test]
        public void BindingParameterAnyObjectToSpecificArrayConversion()
        {
            var exception = new RuntimeException("foo");
            _parameters.Add("FavoriteExceptions", exception);

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual(new Exception[] { exception }, _cmdlet.FavoriteExceptions);
        }

        [Test]
        public void BindingParameterArrayElementConversion()
        {
            int two = 2;
            _parameters.Add("FavoriteNumbers", two);

            _binder.BindCommandLineParameters(_parameters);

            Assert.AreEqual(new double[] { (double) two }, _cmdlet.FavoriteNumbers);
        }
    }
}

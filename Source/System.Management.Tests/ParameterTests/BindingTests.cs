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
        private CmdletParameterBinder _binder = null;
        private TestParameterCommand _cmdlet = null;

        [SetUp]
        public void LoadCmdInfo()
        {
            _info = TestParameterCommand.CreateCmdletInfo();
            _cmdlet = new TestParameterCommand();
            _binder = new CmdletParameterBinder(_info, _cmdlet);
        }

        [Test]
        public void BindingField()
        {
            var parameters = new CommandParameterCollection {
                { "Name", "John" },
                { "Variable", "foo" } // mandatory
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("John", _cmdlet.Name);
        }

        [Test]
        public void BindingFieldAlias()
        {
            var parameters = new CommandParameterCollection {
                { "fn", "John" },
                { "Variable", "foo" } // mandatory
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("John", _cmdlet.Name);
        }

        [Test]
        public void BindingParameter()
        {
            var parameters = new CommandParameterCollection {
                { "InputObject", 10 },
                { "Variable", "foo" } // mandatory
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("10", _cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingParameterByPipeline()
        {
            var parameters = new CommandParameterCollection {
                { "Variable", "foo" } // mandatory
            };

            _binder.BindCommandLineParameters(parameters);
            _binder.BindPipelineParameters(10, true);

            Assert.AreEqual("10", _cmdlet.InputObject.ToString());
        }

        [Test]
        public void BindingParameterAlias()
        {
            var parameters = new CommandParameterCollection {
                { "Path", "a path" }
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("a path", _cmdlet.FilePath.ToString());
        }

        [Test]
        public void LookupOfAmbiguousParameterFails()
        {
            var parameters = new CommandParameterCollection {
                { "i", 10 }
            };

            Assert.Throws(typeof(ParameterBindingException), delegate() {
                _binder.BindCommandLineParameters(parameters);
            });
        }

        [Test]
        public void BindingNonSwitch()
        {
            var parameters = new CommandParameterCollection {
                { "Name", "John" },
                { "Variable", "foo" }
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("John", _cmdlet.Name);
            Assert.IsFalse(_cmdlet.Recurse.ToBool());
        }

        [TestCase("Variable")]
        [TestCase("FilePath")]
        public void BindingCombinationAllSet(string mandatory)
        {
            var parameters = new CommandParameterCollection {
                { "Name", "John" },
                { "Recurse", true },
                { mandatory, "foo" } // chooses the parameter set
            };

            _binder.BindCommandLineParameters(parameters);

            // as the values are in both sets, it should be always set
            Assert.AreEqual("John", _cmdlet.Name);
            Assert.IsTrue(_cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingCombinationNonDefaultSet()
        {
            var parameters = new CommandParameterCollection {
                { "Variable", "a" },
                { "Recurse", true }
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("a", _cmdlet.Variable);
            Assert.IsTrue(_cmdlet.Recurse.ToBool());
        }

        [Test]
        public void BindingParameterSetSelectionSingle()
        {
            var parameters = new CommandParameterCollection {
                { "FilePath", "a path" }
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test]
        public void BindingParameterSetSelectionSingleAlias()
        {
            var parameters = new CommandParameterCollection {
                { "PSPath", "a path" }
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test]
        public void BindingParameterSetSelectionDoubleShouldFail()
        {
            var parameters = new CommandParameterCollection {
                { "Variable", "test" },
                { "FilePath", "a path" }
            };

            Assert.Throws(typeof(ParameterBindingException), delegate()
            {
                    _binder.BindCommandLineParameters(parameters);
            });
        }

        [Test]
        public void BindingParameterTwiceShouldFail()
        {
            var parameters = new CommandParameterCollection {
                { "Variable", "foo" },
                { "Variable", "bar" }
            };
            Assert.Throws(typeof(ParameterBindingException), delegate()
            {
                _binder.BindCommandLineParameters(parameters);
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
            var parameters = new CommandParameterCollection {
                { null, name }
            };

            var ex = Assert.Throws(typeof(ParameterBindingException), delegate()
            {
                _binder.BindCommandLineParameters(parameters);
            });
            StringAssert.Contains("FilePath", ex.Message); // should complain that no "FilePath" was provided
        }

        [Test]
        public void BindingParameterByPositionSelectsDefaultParameterSet()
        {
            var name = "foo";
            var path = "a path";
            var parameters = new CommandParameterCollection {
                { null, name },
                { null, path }
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual(path, _cmdlet.FilePath);
            Assert.AreEqual(name, _cmdlet.Name);
            Assert.AreEqual("File", _cmdlet.ParameterSetName);
        }

        [Test]
        public void BindingParameterByPositionAfterSetSelection()
        {
            var varname = "foo";
            var parameters = new CommandParameterCollection {
                { "ConstVar", true}, // switch parameter, should select "Variable" set
                { null, varname }
            };

            _binder.BindCommandLineParameters(parameters);

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
            var parameters = new CommandParameterCollection {
                { "FavoriteNumbers", pi}
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual(new double[] { pi }, _cmdlet.FavoriteNumbers);
        }

        [Test]
        public void BindingParameterAnyObjectToSpecificArrayConversion()
        {
            var exception = new RuntimeException("foo");
            var parameters = new CommandParameterCollection {
                { "FavoriteExceptions", exception}
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual(new Exception[] { exception }, _cmdlet.FavoriteExceptions);
        }

        [Test]
        public void BindingParameterArrayElementConversion()
        {
            int two = 2;
            var parameters = new CommandParameterCollection {
                { "FavoriteNumbers", two}
            };

            _binder.BindCommandLineParameters(parameters);

            Assert.AreEqual(new double[] { (double) two }, _cmdlet.FavoriteNumbers);
        }
    }
}

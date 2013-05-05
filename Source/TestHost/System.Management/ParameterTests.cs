// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace TestHost
{
    [TestFixture]
    public class ParameterTests
    {
        [Test]
        public void ParametersByName1()
        {
            var results = TestHost.Execute("$a = 10; Get-Variable -Name a");

            Assert.AreEqual("$a = 10" + Environment.NewLine, results);
        }

        [Test, Explicit("This currently fails because GetDateCommand has default parameter set: net but no parameters in the set ( missing -Format param.).")]
        public void ParametersByName2()
        {
            var result1 = TestHost.Execute("Get-Date");
            var result2 = TestHost.Execute("$d = Get-Date; Get-Date -Date $d");

            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void ParametersByPrefix()
        {
            var results = TestHost.Execute("$a = 10; Get-Variable -Nam a");

            Assert.AreEqual("$a = 10" + Environment.NewLine, results);
        }

        [Test]
        public void ParametersByAlias()
        {
            Assert.DoesNotThrow(delegate() {
                TestHost.Execute("Get-Process -ProcessName mono");
            });
        }

        [Test]
        public void ParametersByAliasPrefix()
        {
            Assert.DoesNotThrow(delegate() {
                TestHost.Execute("Get-Process -ProcessN mono");
            });
        }

        [Test]
        [ExpectedException]
        public void ParametersInvalid()
        {
             TestHost.Execute("Get-Process -Procecc mono");
        }
    }
}

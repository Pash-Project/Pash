// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    [TestFixture]
    public class ParameterTests
    {
        [Test]
        public void ParametersByName()
        {
            var results = TestHost.Execute("$a = 10; Get-Variable -Name a");

            Assert.AreEqual("$a = 10" + Environment.NewLine, results);
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class WhereObjectTests : ReferenceTestBase
    {
        [Test]
        public void FilterScriptBlock()
        {
            string result = ReferenceHost.Execute("1,2,3,4 | Where-Object { $_ -ge 3 }");

            Assert.AreEqual(NewlineJoin("3", "4"), result);
        }

        [Test]
        public void FilterScriptBlockUsingQuestionMarkAlias()
        {
            string result = ReferenceHost.Execute("1,2,3,4 | ? { $_ -le 2 }");

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void FilterScriptBlockUsingWhereAlias()
        {
            string result = ReferenceHost.Execute("1,2,3,4 | where { $_ -le 2 }");

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void FilterScriptBlockThatReturnsNullInsteadOfBoolean()
        {
            string result = ReferenceHost.Execute("1,2,3,4 | where { $null }");

            Assert.AreEqual("", result);
        }
    }
}

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

        [Test]
        public void PropertyNameValueEquals()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Command | where -Property Name -eq -Value 'Where-Object'",
                "$c.Name"
            });

            Assert.AreEqual("Where-Object" + Environment.NewLine, result);
        }

        [Test]
        public void PropertyNameWithDifferentCaseEqualsValue()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Command | where -Property name -eq -Value 'Where-Object'",
                "$c.Name"
            });

            Assert.AreEqual("Where-Object" + Environment.NewLine, result);
        }

        [Test]
        public void PropertyValueWithDifferentCaseEqualsValue()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Command | where -Property Name -eq -Value 'where-object'",
                "$c.Name"
            });

            Assert.AreEqual("Where-Object" + Environment.NewLine, result);
        }

        [Test]
        public void PropertyValueEqualsWithoutUsingNamedParameters()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Command | where name -eq 'where-object'",
                "$c.Name"
            });

            Assert.AreEqual("Where-Object" + Environment.NewLine, result);
        }

        [Test]
        public void UnknownPropertyEqualsNonNullValueFiltersOutAllValues()
        {
            string result = ReferenceHost.Execute("1,2,3 | where -Property UnknownProperty -eq -Value 1");

            Assert.AreEqual("", result);
        }

        [Test]
        public void UnknownPropertyEqualsNullValueReturnsAllValues()
        {
            string result = ReferenceHost.Execute("1,2,3 | where -Property UnknownProperty -eq -Value $null");

            Assert.AreEqual(NewlineJoin("1", "2", "3"), result);
        }

        [Test]
        public void NullPipelineObjectMatchesNullValue()
        {
            string result = ReferenceHost.Execute("$null | where -Property UnknownProperty -eq -Value $null");

            Assert.AreEqual(Environment.NewLine, result);
        }
    }
}

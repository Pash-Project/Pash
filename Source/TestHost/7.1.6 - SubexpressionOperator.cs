// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    public class SubExpressionOperatorTests
    {
        [Test]
        [TestCase("$(2)", "2")]
        [TestCase("$(1+2)", "3")]
        //[TestCase("$()", "")] // Ignore. Works with PowerShell. From spec "If statement-list is omitted, the result is $null."
        public void SingleStatementInSubExpression(string input, string expectedResult)
        {
            var result = TestHost.Execute(true, input);

            Assert.AreEqual(expectedResult + Environment.NewLine, result);
        }

        [Test]
        [TestCase("$($i = 10)", Description = "# pipeline gets nothing")]
        [TestCase("$(($i = 10))", "10", Description = "# pipeline gets int 10")]
        [TestCase("$($i = 10; $j)", "20", Description = "# pipeline gets int 20")]
        [TestCase("$(($i = 10); $j)", "10", "20", Description = "# pipeline gets [object[]](10,20)")]
        [TestCase("$(($i = 10); ++$j)", "10", Description = "# pipeline gets int 10")]
        [TestCase("$(($i = 10); (++$j))", "10", "21", Description = "# pipeline gets [object[]](10,21)")] // spec says (10,22)
        [TestCase("$($i = 10; ++$j)", Description = "# pipeline gets nothing")]
        [TestCase("$(2,4,6)", "2", "4", "6", Description = "# pipeline gets [object[]](2,4,6)")]
        public void SubExpressionExamplesFromSpec(string input, params string[] expected)
        {
            var result = TestHost.Execute(
                @"$j = 20",
                input
                );

            Assert.AreEqual(expected.Any() ? expected.JoinString(Environment.NewLine) + Environment.NewLine : "", result);
        }

        [Test]
        public void SubExpressionReturnsMultipleItemsAsObjectArray()
        {
            var result = TestHost.Execute(
                "$i = $(2,4,6)",
                "$i.GetType().Name");

            Assert.AreEqual("Object[]" + Environment.NewLine, result);
        }

        [Test]
        public void VariableAssignmentSubExpressionReturnsNull()
        {
            string result = TestHost.Execute("$($i = 10) -eq $null");

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void VariableAssignedToResultOfVariableAssignmentSubExpressionIsNull()
        {
            string result = TestHost.Execute(
                "$f = $($i = 10)",
                "$f -eq $null");

            Assert.AreEqual("True" + Environment.NewLine, result);
        }
    }
}

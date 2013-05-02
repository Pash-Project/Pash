// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    class ArraySubexpressionOperator
    {
        [Test]
        [TestCase(@"@($i = 10)", 0, Description = "10 not written to pipeline, result is array of 0")]
        [TestCase(@"@(($i = 10))", 1, "10", Description = "pipeline gets 10, result is array of 1")]
        [TestCase(@"@($i = 10; $j)", 1, "20", Description = "10 not written to pipeline, result is array of 1")]
        [TestCase(@"@(($i = 10); $j)", 2, "10", "20", Description = "pipeline gets 10, result is array of 2")]
        [TestCase(@"@(($i = 10); ++$j)", 1, "10", Description = "pipeline gets 10, result is array of 1")]
        [TestCase(@"@(($i = 10); (++$j))", 2, "10", "21", Description = "pipeline gets both values, result is array of 2")]
        [TestCase(@"@($i = 10; ++$j)", 0, Description = "pipeline gets nothing, result is array of 0")]
        public void Test1(string input, int expectedCount, params string[] expected)
        {
            {
                var result = TestHost.Execute(
                    @"$j = 20",
                    input
                    );

                Assert.AreEqual(expected.Any() ? expected.JoinString(Environment.NewLine) + Environment.NewLine : "", result);
            }

            // TODO: `Count` is an alias property
            // TODO: single-element arrays are treated as singletons. Bad!

            //{
            //    var result = TestHost.Execute(
            //        @"$j = 20",
            //        input + ".Count"
            //        );

            //    Assert.AreEqual(expectedCount.ToString() + Environment.NewLine, result);
            //}
        }

        [Test]
        [TestCase("$a")]
        [TestCase(@"@($a)		 				# result is the same array of 3")]
        [TestCase(@"@(@($a)) 				# result is the same array of 3")]
        public void Onedimensional(string input)
        {
            var result = TestHost.Execute(
                @"$a = @(2,4,6)			# result is array of 3",
                input
                );

            Assert.AreEqual(
                "2" + Environment.NewLine +
                "4" + Environment.NewLine +
                "6" + Environment.NewLine
                , result);
        }
    }
}

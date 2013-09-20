// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    public class PatternMatchingOperators
    {
        [TestCase(@"'Monday' -match 'mon'", "True")]
        [TestCase(@"'Tuesday' -match 'mon'", "False")]
        [TestCase(@"$test = 'Monday'; $pattern = 'mon'; $test -match $pattern", "True")]
        public void Match(string input, string expected)
        {
            string result = TestHost.Execute(input);

            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [TestCase(@"'Monday' -match 'mon' | out-null; $matches[0]", "Mon")]
        [TestCase(@"'Tuesday' -match 'tue' | out-null; $matches[0]", "Tue")]
        [TestCase(@"'01/02/2000 Desc' -match '(.*) (.*)' | out-null; $matches[1]", "01/02/2000")]
        public void Matches(string input, string expected)
        {
            string result = TestHost.Execute(input);

            Assert.AreEqual(expected + Environment.NewLine, result);
        }
    }
}

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
    }
}

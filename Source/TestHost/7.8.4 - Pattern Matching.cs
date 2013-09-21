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

        // Capturing group
        [TestCase(@"'Monday' -match 'mon' | out-null; $matches[0]", "Mon")]
        [TestCase(@"'Tuesday' -match 'tue' | out-null; $matches[0]", "Tue")]
        [TestCase(@"'01/02/2000 Desc' -match '(.*) (.*)' | out-null; $matches[1]", "01/02/2000")]
        // Named capturing group
        [TestCase(@"'Monday' -match '(?<word>[a-z]+)' | out-null; $matches['word']", "Monday")]
        // Numbered groups not as string
        [TestCase(@"'abc' -match '(a)(b)(c)' | out-null; $matches['0'] -eq $null", "True")]
        [TestCase(@"'abc' -match '(a)(b)(c)' | out-null; $matches['1'] -eq $null", "True")]
        [TestCase(@"'abc' -match '(a)(b)(c)' | out-null; $matches['2'] -eq $null", "True")]
        [TestCase(@"'abc' -match '(a)(b)(c)' | out-null; $matches['3'] -eq $null", "True")]
        // Mixed named and numbered groups
        [TestCase(@"'abc' -match '(a)(b)(?<x>c)' | out-null; $matches[1]", "a")]
        [TestCase(@"'abc' -match '(a)(b)(?<x>c)' | out-null; $matches[2]", "b")]
        [TestCase(@"'abc' -match '(a)(b)(?<x>c)' | out-null; $matches[3] -eq $null", "True")]
        [TestCase(@"'abc' -match '(a)(b)(?<x>c)' | out-null; $matches['x']", "c")]
        public void Matches(string input, string expected)
        {
            string result = TestHost.Execute(input);

            Assert.AreEqual(expected + Environment.NewLine, result);
        }
    }
}

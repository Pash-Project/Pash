// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests.API
{
    [TestFixture]
    public class WildcardPatternTests
    {
        [Test]
        [TestCase("", false)]
        [TestCase("a", false)]
        [TestCase("a-b", false)]

        [TestCase("*", true)]
        [TestCase("Test*", true)]
        [TestCase("*Test", true)]
        [TestCase("Te*st", true)]

        [TestCase("?", true)]
        [TestCase("a?", true)]
        [TestCase("?b", true)]
        [TestCase("a?b", true)]

        [TestCase("[", true)]
        [TestCase("[a", true)]
        [TestCase("]", true)]
        [TestCase("a]", true)]
        [TestCase("[ab]", true)]
        [TestCase("a[ab]", true)]
        [TestCase("[ab]c", true)]
        [TestCase("a[ab]c", true)]

        [TestCase("[a-c]", true)]
        [TestCase("a[a-c]", true)]
        [TestCase("[a-c]c", true)]
        [TestCase("a[a-c]c", true)]
        public void TestContainsWildcardCharactersSimple(string input, bool expected)
        {
            if (expected)
                Assert.IsTrue(WildcardPattern.ContainsWildcardCharacters(input));
            else
                Assert.IsFalse(WildcardPattern.ContainsWildcardCharacters(input));
        }

        [Test]
        [TestCase("`[", false)]
        [TestCase("`[a", false)]
        [TestCase("a`[", false)]
        [TestCase("a`[a", false)]

        [TestCase("`]", false)]
        [TestCase("`]a", false)]
        [TestCase("a`]", false)]
        [TestCase("a`]a", false)]

        [TestCase("`*", false)]
        [TestCase("`*a", false)]
        [TestCase("a`*", false)]
        [TestCase("a`*a", false)]

        [TestCase("`?", false)]
        [TestCase("`?a", false)]
        [TestCase("a`?", false)]
        [TestCase("a`?a", false)]
        public void TestContainsWildcardCharactersEscaped(string input, bool expected)
        {
            if (expected)
                Assert.IsTrue(WildcardPattern.ContainsWildcardCharacters(input));
            else
                Assert.IsFalse(WildcardPattern.ContainsWildcardCharacters(input));
        }

        [Test]
        [TestCase("``[", true)]
        [TestCase("``[a", true)]
        [TestCase("a``[", true)]
        [TestCase("a``[a", true)]

        [TestCase("``]", true)]
        [TestCase("``]a", true)]
        [TestCase("a``]", true)]
        [TestCase("a``]a", true)]

        [TestCase("``*", true)]
        [TestCase("``*a", true)]
        [TestCase("a``*", true)]
        [TestCase("a``*a", true)]

        [TestCase("``?", true)]
        [TestCase("``?a", true)]
        [TestCase("a``?", true)]
        [TestCase("a``?a", true)]
        public void TestContainsWildcardCharactersDoubleEscape(string input, bool expected)
        {
            if (expected)
                Assert.IsTrue(WildcardPattern.ContainsWildcardCharacters(input));
            else
                Assert.IsFalse(WildcardPattern.ContainsWildcardCharacters(input));
        }

        [Test]
        [TestCase("````*", true)]
        [TestCase("`````*", false)]
        [TestCase("````````````*", true)]
        [TestCase("`````````````*", false)]

        [TestCase("````?", true)]
        [TestCase("`````?", false)]
        [TestCase("````````````?", true)]
        [TestCase("`````````````?", false)]

        [TestCase("````[", true)]
        [TestCase("`````[", false)]
        [TestCase("````````````[", true)]
        [TestCase("`````````````[", false)]

        [TestCase("````]", true)]
        [TestCase("`````]", false)]
        [TestCase("````````````]", true)]
        [TestCase("`````````````]", false)]
        public void TestContainsWildcardCharactersMultiEscape(string input, bool expected)
        {
            if (expected)
                Assert.IsTrue(WildcardPattern.ContainsWildcardCharacters(input));
            else
                Assert.IsFalse(WildcardPattern.ContainsWildcardCharacters(input));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("abc", "abc")]
        [TestCase("@(#&!^$", "@(#&!^$")]
        public void TestEscapeSimpleString(string input, string expected)
        {
            Assert.AreEqual(expected, WildcardPattern.Escape(input));
        }

        [Test]
        [TestCase("*", "`*")]
        [TestCase("a*", "a`*")]

        [TestCase("?", "`?")]
        [TestCase("a?", "a`?")]

        [TestCase("[", "`[")]
        [TestCase("]", "`]")]
        [TestCase("a[a-c]b", "a`[a-c`]b")]
        public void TestEscapeWildcardCharacters(string input, string expected)
        {
            Assert.AreEqual(expected, WildcardPattern.Escape(input));
        }

        [Test]
        [TestCase("`*", "``*")]
        [TestCase("``*", "```*")]

        [TestCase("`?", "``?")]
        [TestCase("``?", "```?")]

        [TestCase("`[", "``[")]
        [TestCase("``[", "```[")]
        [TestCase("`]", "``]")]
        [TestCase("``]", "```]")]
        public void TestEscapeEscapedWildcardCharacters(string input, string expected)
        {
            Assert.AreEqual(expected, WildcardPattern.Escape(input));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("abc", "abc")]
        [TestCase("@(#&!^$", "@(#&!^$")]
        public void TestUnescapeSimpleString(string input, string expected)
        {
            Assert.AreEqual(expected, WildcardPattern.Unescape(input));
        }

        [Test]
        [TestCase("`*", "*")]
        [TestCase("a`*", "a*")]

        [TestCase("`?", "?")]
        [TestCase("a`?", "a?")]

        [TestCase("`[", "[")]
        [TestCase("`]", "]")]
        [TestCase("a`[a-c`]b", "a[a-c]b")]
        public void TestUnescapeWildcardCharacters(string input, string expected)
        {
            Assert.AreEqual(expected, WildcardPattern.Unescape(input));
        }

        [Test]
        [TestCase("``*", "`*")]
        [TestCase("```*", "`*")]

        [TestCase("``?", "`?")]
        [TestCase("```?", "`?")]

        [TestCase("``[", "`[")]
        [TestCase("```[", "`[")]
        [TestCase("``]", "`]")]
        [TestCase("```]", "`]")]
        public void TestUnescapeEscapedWildcardCharacters(string input, string expected)
        {
            Assert.AreEqual(expected, WildcardPattern.Unescape(input));
        }

        [Test]
        [TestCase("", "", true)]
        [TestCase("", "x", false)]
        [TestCase("a", "a", true)]
        [TestCase("a", "ab", false)]
        [TestCase("a", "ba", false)]
        [TestCase("a", "aa", false)]
        public void TestIsMatchLiteralString(string pattern, string input, bool expected)
        {
            if (expected)
                Assert.IsTrue(new WildcardPattern(pattern).IsMatch(input));
            else
                Assert.IsFalse(new WildcardPattern(pattern).IsMatch(input));
        }

        [Test]
        [TestCase("a*", "a", true)]
        [TestCase("a*", "abc", true)]
        [TestCase("a*", "b", false)]
        [TestCase("a*", "ba", false)]

        [TestCase("*a", "a", true)]
        [TestCase("*a", "cba", true)]
        [TestCase("*a", "b", false)]
        [TestCase("*a", "abc", false)]

        [TestCase("a*a", "a", false)]
        [TestCase("a*a", "aa", true)]
        [TestCase("a*a", "aba", true)]
        [TestCase("a*a", "abcba", true)]
        [TestCase("a*a", "abab", false)]
        [TestCase("a*a", "baba", false)]

        [TestCase("*a*", "a", true)]
        [TestCase("*a*", "abc", true)]
        [TestCase("*a*", "cba", true)]
        [TestCase("*a*", "cbabc", true)]
        [TestCase("*a*", "b", false)]

        [TestCase("a?", "a", false)]
        [TestCase("a?", "abc", false)]
        [TestCase("a?", "ab", true)]
        [TestCase("a?", "b", false)]
        [TestCase("a?", "ba", false)]

        [TestCase("?a", "a", false)]
        [TestCase("?a", "cba", false)]
        [TestCase("?a", "ba", true)]
        [TestCase("?a", "b", false)]
        [TestCase("?a", "ab", false)]

        [TestCase("a?a", "a", false)]
        [TestCase("a?a", "aa", false)]
        [TestCase("a?a", "aba", true)]
        [TestCase("a?a", "abcba", false)]
        [TestCase("a?a", "abab", false)]
        [TestCase("a?a", "baba", false)]

        [TestCase("?a?", "a", false)]
        [TestCase("?a?", "cbabc", false)]
        [TestCase("?a?", "bab", true)]
        [TestCase("?a?", "b", false)]
        [TestCase("?a?", "aba", false)]

        [TestCase("a[bc]", "a", false)]
        [TestCase("a[bc]", "aa", false)]
        [TestCase("a[bc]", "ab", true)]
        [TestCase("a[bc]", "ac", true)]
        [TestCase("a[bc]", "ad", false)]
        [TestCase("a[bc]", "abc", false)]
        [TestCase("a[bc]", "acb", false)]

        [TestCase("[bc]a", "a", false)]
        [TestCase("[bc]a", "aa", false)]
        [TestCase("[bc]a", "ba", true)]
        [TestCase("[bc]a", "ca", true)]
        [TestCase("[bc]a", "da", false)]
        [TestCase("[bc]a", "cba", false)]
        [TestCase("[bc]a", "bca", false)]

        [TestCase("a[bc]a", "a", false)]
        [TestCase("a[bc]a", "aa", false)]
        [TestCase("a[bc]a", "aaa", false)]
        [TestCase("a[bc]a", "aba", true)]
        [TestCase("a[bc]a", "aca", true)]
        [TestCase("a[bc]a", "ada", false)]
        [TestCase("a[bc]a", "abba", false)]
        [TestCase("a[bc]a", "acca", false)]

        [TestCase("[bc]a[de]", "a", false)]
        [TestCase("[bc]a[de]", "aa", false)]
        [TestCase("[bc]a[de]", "aaa", false)]
        [TestCase("[bc]a[de]", "bac", false)]
        [TestCase("[bc]a[de]", "bad", true)]
        [TestCase("[bc]a[de]", "bae", true)]
        [TestCase("[bc]a[de]", "baf", false)]
        [TestCase("[bc]a[de]", "cac", false)]
        [TestCase("[bc]a[de]", "cad", true)]
        [TestCase("[bc]a[de]", "cae", true)]
        [TestCase("[bc]a[de]", "caf", false)]
        [TestCase("[bc]a[de]", "abad", false)]
        [TestCase("[bc]a[de]", "bada", false)]

        [TestCase("a[b-d]", "a", false)]
        [TestCase("a[b-d]", "aa", false)]
        [TestCase("a[b-d]", "ab", true)]
        [TestCase("a[b-d]", "ac", true)]
        [TestCase("a[b-d]", "ad", true)]
        [TestCase("a[b-d]", "ae", false)]
        [TestCase("a[b-d]", "abx", false)]
        [TestCase("a[b-d]", "acx", false)]
        [TestCase("a[b-d]", "adx", false)]

        [TestCase("[b-d]a", "a", false)]
        [TestCase("[b-d]a", "aa", false)]
        [TestCase("[b-d]a", "ba", true)]
        [TestCase("[b-d]a", "ca", true)]
        [TestCase("[b-d]a", "da", true)]
        [TestCase("[b-d]a", "ea", false)]
        [TestCase("[b-d]a", "xba", false)]
        [TestCase("[b-d]a", "xca", false)]
        [TestCase("[b-d]a", "xda", false)]

        [TestCase("a[b-d]a", "a", false)]
        [TestCase("a[b-d]a", "aa", false)]
        [TestCase("a[b-d]a", "aaa", false)]
        [TestCase("a[b-d]a", "aba", true)]
        [TestCase("a[b-d]a", "aca", true)]
        [TestCase("a[b-d]a", "ada", true)]
        [TestCase("a[b-d]a", "aea", false)]
        [TestCase("a[b-d]a", "abba", false)]
        [TestCase("a[b-d]a", "acca", false)]
        [TestCase("a[b-d]a", "adda", false)]

        [TestCase("[b-d]a[e-f]", "a", false)]
        [TestCase("[b-d]a[e-f]", "aa", false)]
        [TestCase("[b-d]a[e-f]", "aaa", false)]
        [TestCase("[b-d]a[e-f]", "bad", false)]
        [TestCase("[b-d]a[e-f]", "bae", true)]
        [TestCase("[b-d]a[e-f]", "baf", true)]
        [TestCase("[b-d]a[e-f]", "baf", true)]
        [TestCase("[b-d]a[e-f]", "bag", false)]
        [TestCase("[b-d]a[e-f]", "cad", false)]
        [TestCase("[b-d]a[e-f]", "cae", true)]
        [TestCase("[b-d]a[e-f]", "caf", true)]
        [TestCase("[b-d]a[e-f]", "caf", true)]
        [TestCase("[b-d]a[e-f]", "cag", false)]
        [TestCase("[b-d]a[e-f]", "dad", false)]
        [TestCase("[b-d]a[e-f]", "dae", true)]
        [TestCase("[b-d]a[e-f]", "daf", true)]
        [TestCase("[b-d]a[e-f]", "daf", true)]
        [TestCase("[b-d]a[e-f]", "dag", false)]
        [TestCase("[b-d]a[e-f]", "abae", false)]
        [TestCase("[b-d]a[e-f]", "baea", false)]
        public void TestIsMatchWildcard(string pattern, string input, bool expected)
        {
            if (expected)
                Assert.IsTrue(new WildcardPattern(pattern).IsMatch(input));
            else
                Assert.IsFalse(new WildcardPattern(pattern).IsMatch(input));
        }
    }
}

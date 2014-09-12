// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pash.Implementation.Native;

namespace System.Management.Tests.ExtensionsTests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void GenerateTest()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, Extensions.Enumerable._.Generate(1, i => i + 1, 3).ToArray());
        }

        [TestCase("foo bar baz", "baz")]
        [TestCase("foo bar baz ", "")]
        [TestCase("foobarbaz", "foobarbaz")]
        [TestCase("foobarbaz 'fuu bar'", "'fuu bar'")]
        [TestCase("foobarbaz 'fuu bar", "'fuu bar")]
        [TestCase("foobarbaz '\"fuu\" bar", "'\"fuu\" bar")]
        [TestCase("foobarbaz \"'fuu baz\" bar", "bar")]
        public void FindingLastUnescapedSpaceWorks(string input, string lastWord)
        {
            int pos = input.LastUnquotedIndexOf(' ');
            Assert.AreEqual(lastWord, input.Substring(pos + 1));
        }
    }
}

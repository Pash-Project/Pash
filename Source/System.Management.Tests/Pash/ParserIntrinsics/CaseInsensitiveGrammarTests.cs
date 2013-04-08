// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Pash.ParserIntrinsics.Tests
{
    [TestFixture]
    public class CaseInsensitiveGrammarTests
    {
        // I had a bug where I got the case-insenstive bool flag backwards!

        class TestGrammar : CaseInsensitiveGrammar { }

        [Test]
        public void ATest()
        {
            var grammar = new TestGrammar();
            Assert.False(grammar.CaseSensitive);
        }
    }
}

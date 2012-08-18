using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace ParserTests
{
    [TestFixture]
    class PowerShellGrammarLexicalPatternsTests
    {
        [Test]
        public void LetterAsTokenCharTest()
        {
            Assert.IsTrue(Regex.IsMatch("x", PowerShellGrammar.LexicalPatterns.generic_token_char));
        }

        [Test]
        public void DigitAsTokenCharTest()
        {
            Assert.IsTrue(Regex.IsMatch("1", PowerShellGrammar.LexicalPatterns.generic_token_char));
        }

        [Test]
        public void SingleQuoteIsNotTokenCharTest()
        {
            Assert.IsFalse(Regex.IsMatch("'", PowerShellGrammar.LexicalPatterns.generic_token_char));
        }
    }
}

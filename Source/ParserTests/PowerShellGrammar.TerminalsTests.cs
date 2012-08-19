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
        public void LetterAsTokenChar()
        {
            Assert.IsTrue(Regex.IsMatch("x", PowerShellGrammar.Terminals.generic_token_char.Pattern));
        }

        [Test]
        public void DigitAsTokenChar()
        {
            Assert.IsTrue(Regex.IsMatch("1", PowerShellGrammar.Terminals.generic_token_char.Pattern));
        }

        [Test]
        public void SingleQuoteIsNotTokenChar()
        {
            Assert.IsFalse(Regex.IsMatch("'", PowerShellGrammar.Terminals.generic_token_char.Pattern));
        }

        [Test]
        public void GenericToken_Match()
        {
            Assert.IsTrue(Regex.IsMatch("foo", "^" + PowerShellGrammar.Terminals.generic_token.Pattern + "$"));
        }

        [Test]
        public void GenericToken_DoubleToken_NotMatch()
        {
            Assert.IsFalse(Regex.IsMatch("foo foo", "^" + PowerShellGrammar.Terminals.generic_token.Pattern + "$"));
        }

        [Test]
        public void RegexInitTest()
        {
            Assert.IsNotNull(PowerShellGrammar.Terminals.input_elements);
            Assert.AreEqual("input_elements", PowerShellGrammar.Terminals.input_elements.Name);
        }

        [Test, Ignore]
        public void TokenizeSimplePromptTest()
        {
            Assert.True(Regex.IsMatch("\"PS> \" + (Get-Location)", "^" + PowerShellGrammar.Terminals.input_elements.Pattern + "$"));
        }

        [Test]
        public void TokenizeStringTest()
        {
            StringAssert.IsMatch("^" + PowerShellGrammar.Terminals.string_literal.Pattern + "$", "\"PS> \"");
        }
    }
}


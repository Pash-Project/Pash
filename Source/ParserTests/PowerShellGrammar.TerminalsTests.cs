using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Pash.ParserIntrinsics;

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
        public void GenericToken_String_NotMatch()
        {
            StringAssert.DoesNotMatch("^" + PowerShellGrammar.Terminals.generic_token.Pattern + "$", "\"PS> \"");
        }

        [Test]
        public void VerbatimStringMatch()
        {
            var matches = Regex.Match("'PS> '", PowerShellGrammar.Terminals.literal.Pattern);
            Assert.True(matches.Success);
            Assert.AreEqual(0, matches.Groups[PowerShellGrammar.Terminals.literal.Name].Index);
            // For any node of Terminal X, you should be able to get `matches.Groups[X]`, which is the whole matched text
            //
            // Not normally useful, since you can do `term.text`, but it fits in to the next assert.
            Assert.AreEqual("'PS> '", matches.Groups[PowerShellGrammar.Terminals.literal.Name].Value);
            // How to get the useful value out a terminal. Consider that there are 4 different accepted single-quote
            // characters that could demarcate a string literal, and we really don't care which one you used - 
            // we just want the contents.
            Assert.AreEqual("PS> ", matches.Groups[PowerShellGrammar.Terminals.verbatim_string_characters.Name].Value);
        }

        // Simply ensure that the reflection used to initialize these fields actually worked
        [Test]
        public void RegexInitTest()
        {
            Assert.IsNotNull(PowerShellGrammar.Terminals.new_lines);
            Assert.AreEqual("new_lines", PowerShellGrammar.Terminals.new_lines.Name);
        }

        [Test]
        public void TokenizeStringTest()
        {
            StringAssert.IsMatch("^" + PowerShellGrammar.Terminals.string_literal.Pattern + "$", "\"PS> \"");
        }
    }
}


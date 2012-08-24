using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Pash.ParserIntrinsics;
using Pash.ParserIntrinsics.Nodes;
using Irony.Parsing;

namespace ParserTests
{
    [TestFixture]
    public class PowerShellGrammarLexicalPatternsTests
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
            AssertIsFullStringMatch(PowerShellGrammar.Terminals.generic_token.Pattern, "foo");
        }

        [Test]
        public void GenericToken_DoubleToken_NotMatch()
        {
            Assert.IsFalse(Regex.IsMatch("foo foo", "^" + PowerShellGrammar.Terminals.generic_token.Pattern + "$"));
        }

        // make sure we don't see string literals as `generic_token`.
        [Test]
        public void GenericToken_String_NotMatch()
        {
            StringAssert.DoesNotMatch("^" + PowerShellGrammar.Terminals.generic_token.Pattern + "$", "\"PS> \"");
        }

        [Test]
        public void VerbatimStringMatch()
        {
            var matches = Regex.Match("'PS> '", PowerShellGrammar.Terminals.verbatim_string_characters.Pattern);
            Assert.True(matches.Success);

            // How to get the useful value out a terminal. Consider that there are 4 different accepted single-quote
            // characters that could demarcate a string literal, and we really don't care which one you used - 
            // we just want the contents.
            Assert.AreEqual("PS> ", matches.Groups[PowerShellGrammar.Terminals.verbatim_string_characters.Name].Value);
        }

        [Test]
        public void DecimalIntegerLiteralTest()
        {
            var matches = Regex.Match("17", PowerShellGrammar.Terminals.decimal_integer_literal.Pattern);
            Assert.True(matches.Success);
            Assert.AreEqual(17.ToString(), matches.Groups[PowerShellGrammar.Terminals.decimal_integer_literal.Name].Value);
        }

        [Test]
        public void HexIntegerLiteralTest()
        {
            var matches = Regex.Match("0x17", PowerShellGrammar.Terminals.hexadecimal_integer_literal.Pattern);
            Assert.True(matches.Success);
            Assert.AreEqual("0x17", matches.Groups[PowerShellGrammar.Terminals.hexadecimal_integer_literal.Name].Value);
        }

        [Test]
        public void HexIntegerLiteralTest2()
        {
            var matches = Regex.Match("0x17", PowerShellGrammar.Terminals.hexadecimal_integer_literal.Pattern);
            Assert.True(matches.Success);
            Assert.AreEqual("0x17", matches.Value);
            Assert.AreEqual("0x17", matches.Groups[PowerShellGrammar.Terminals.hexadecimal_integer_literal.Name].Value);
        }

        [Test]
        public void HexIntegerLiteralTest3()
        {
            var matches = Regex.Match("0x17", PowerShellGrammar.Terminals.hexadecimal_integer_literal.Pattern);
            Assert.True(matches.Success);
            Assert.AreEqual("0x17", matches.Groups[PowerShellGrammar.Terminals.hexadecimal_integer_literal.Name].Value);
        }

        [Test]
        public void HEXIntegerLiteralTest()
        {
            var matches = Regex.Match("0X17", PowerShellGrammar.Terminals.hexadecimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            Assert.True(matches.Success);
            Assert.AreEqual("0X17", matches.Groups[PowerShellGrammar.Terminals.hexadecimal_integer_literal.Name].Value);
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
            AssertIsFullStringMatch(PowerShellGrammar.Terminals.expandable_string_literal.Pattern, "\"PS> \"");
        }

        [Test]
        public void generic_token_AstConfig_Test()
        {
            Assert.AreEqual(typeof(generic_token_node), PowerShellGrammar.Terminals.generic_token.AstConfig.NodeType);
            Assert.IsFalse((PowerShellGrammar.Terminals.generic_token.Flags & TermFlags.NoAstNode) != 0);
        }

        // Use this to confirm that the regex matches greedily enough.
        static void AssertIsFullStringMatch(string pattern, string input)
        {
            var matches = new Regex(pattern).Match(input);
            Assert.True(matches.Success, "failed to match '{0}' to '{1}'", pattern, input);
            Assert.AreEqual(input, matches.Value, "failed to match the entire input");
        }
    }
}


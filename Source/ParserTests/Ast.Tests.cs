using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using StringExtensions;
using Pash.ParserIntrinsics;
using Pash.ParserIntrinsics.Nodes;

namespace ParserTests
{
    [TestFixture]
    class AstTests
    {
        [Test]
        public void VerbatimStringLiteralExpression()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("'PS> '");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var result = ((_node)parseTree.Root.AstNode).GetValue(null);

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual("PS> ", result);
        }

        [Test]
        public void AdditiveExpression_Add()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("'x' + 'y'");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var result = ((_node)parseTree.Root.AstNode).GetValue(null);

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual("xy", result);
        }

        [Test]
        public void ParenthesizedExpression()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("(Get-Location)");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            //var result = ((_node)parseTree.Root.AstNode).GetValue(null);

            //Assert.IsInstanceOf<string>(result);
            //Assert.AreEqual(@"C:\", result);
        }
    }
}

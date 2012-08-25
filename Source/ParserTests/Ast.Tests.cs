using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using Extensions.String;
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

            var result = ((_node)parseTree.Root.AstNode).Execute(null, null);

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

            var result = ((_node)parseTree.Root.AstNode).Execute(null, null);

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

            // TODO: Find a better example of executing a cmdlet in parens, with a reliable output
            //var result = ((_node)parseTree.Root.AstNode).GetValue(null);

            //Assert.IsInstanceOf<string>(result);
            //Assert.AreEqual(@"C:\", result);
        }

        [Test]
        public void DecimalIntegerTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("7");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var result = ((_node)parseTree.Root.AstNode).Execute(null, null);

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(7, result);
        }

        [Test]
        public void HexIntegerTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("0xA");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var result = ((_node)parseTree.Root.AstNode).Execute(null, null);

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(10, result);
        }

        [Test]
        public void AdditionTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("7 + 0xA");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var result = ((_node)parseTree.Root.AstNode).Execute(null, null);

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(17, result);
        }
    }
}

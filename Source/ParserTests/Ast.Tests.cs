using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using Extensions.String;
using Pash.ParserIntrinsics;
using Pash.ParserIntrinsics.AstNodes;
using System.Collections;
using System.Management.Automation;

namespace ParserTests
{
    [TestFixture]
    class AstTests
    {
        [Test]
        public void VerbatimStringLiteralExpression()
        {
            var result = ExecuteInput("'PS> '");

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual("PS> ", result);
        }

        [Test]
        public void AdditiveExpression_Add()
        {
            var result = ExecuteInput("'x' + 'y'");

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
            //var result = ((_astnode)parseTree.Root.AstNode).GetValue(null);

            //Assert.IsInstanceOf<string>(result);
            //Assert.AreEqual(@"C:\", result);
        }

        [Test]
        public void DecimalIntegerTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var result = ExecuteInput("7");

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(7, result);
        }

        [Test]
        public void NegativeIntegerTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var result = ExecuteInput("-7");

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(-7, result);
        }

        [Test]
        public void HexIntegerTest()
        {
            var result = ExecuteInput("0xA");

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(10, result);
        }

        [Test]
        public void AdditionTest()
        {
            var result = ExecuteInput("7 + 0xA");

            Assert.IsInstanceOf<int>(result, "Result is '{0}'", result);
            Assert.AreEqual(17, result);
        }

        [Test]
        public void ArrayRangeTest()
        {
            //// 7.4 Range operator
            //// Examples:
            //// 
            ////     1..10              # ascending range 1..10
            {
                var result = ((PSObject[])ExecuteInput("1..10"))
                    .Select(o => o.BaseObject);
                Assert.AreEqual(1, result.First());
                Assert.AreEqual(10, result.Last());
                Assert.AreEqual(2, result.Skip(1).First());
            }

            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, ((PSObject[])ExecuteInput("3..1")).Select(o => o.BaseObject));

            ////    -500..-495          # descending range -500..-495
            CollectionAssert.AreEqual(new[] { -500, -499, -498, -497, -496, -495 }, ((PSObject[])ExecuteInput("-500..-495")).Select(o => o.BaseObject));

            ////     16..16             # seqeunce of 1
            CollectionAssert.AreEqual(new[] { 16 }, ((PSObject[])ExecuteInput("16..16")).Select(o => o.BaseObject));

            ////     
            ////     $x = 1.5
            ////     $x..5.40D          # ascending range 2..5
            ////     
            ////     $true..3           # ascending range 1..3
            ////     -2..$null          # ascending range -2..0
            ////    "0xf".."0xa"        # descending range 15..10           
        }

        [Test, Ignore]
        public void UnaryMinusTest()
        {
            ////    7.2.5 Unary minus
            // TODO:
            ////    Examples:
            ////    
            ////    -$true         # type int, value -1
            ////    -123L          # type long, value -123
            ////    -0.12340D      # type decimal, value -0.12340
        }


        [Test]
        public void ArrayLiteralTest()
        {
            var result = ((PSObject[])ExecuteInput("1,3,3")).Select(o => o.BaseObject);

            CollectionAssert.AreEqual(new[] { 1, 3, 3 }, result);
        }

        [Test, Ignore("bug")]
        public void JaggedArrayTest()
        {
            var result = ((PSObject[])ExecuteInput("$x = 1,2; 3,$x")).Select(o => o.BaseObject);

            Assert.AreEqual(2, result.Count());
        }

        static object ExecuteInput(string s)
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse(s);

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            return ((_astnode)parseTree.Root.AstNode).Execute(null, null);
        }
    }
}

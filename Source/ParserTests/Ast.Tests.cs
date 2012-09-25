using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using Extensions.String;
using Pash.ParserIntrinsics;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ParserTests
{
    [TestFixture]
    class AstTests
    {

        [Test, ExpectedException(typeof(PowerShellGrammar.ParseException))]
        public void ParseError()
        {
            ParseInput("$");
        }

        [Test]
        public void HashTable0()
        {
            HashtableAst hashtableAst = ParseInput("@{ }")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(0, hashtableAst.KeyValuePairs.Count);
        }

        [Test]
        public void HashTable1()
        {
            HashtableAst hashtableAst = ParseInput("@{ 'a' = 'b' }")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(1, hashtableAst.KeyValuePairs.Count);
            dynamic keyValuePair = hashtableAst.KeyValuePairs.Single();

            StringConstantExpressionAst nameAst = keyValuePair.Item1;
            StringConstantExpressionAst valueAst = keyValuePair.Item2.PipelineElements[0].Expression;

            Assert.AreEqual("a", nameAst.Value);
            Assert.AreEqual("b", valueAst.Value);
        }

        [Test]
        public void HashTable2()
        {
            HashtableAst hashtableAst = ParseInput("@{ a = b ; c = d }")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(2, hashtableAst.KeyValuePairs.Count);
        }

        [Test]
        public void HashTableIntegerKey()
        {
            HashtableAst hashtableAst = ParseInput("@{ 10 = 'b' }")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0]
                    .Expression;

            var nameAst = (ConstantExpressionAst)hashtableAst.KeyValuePairs.Single().Item1;

            Assert.AreEqual(10, nameAst.Value);
        }

        [Test]
        public void HashTableUnquotedName()
        {
            HashtableAst hashtableAst = ParseInput("@{ a = 'b' }")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0]
                    .Expression;

            var nameAst = (StringConstantExpressionAst)hashtableAst.KeyValuePairs.Single().Item1;

            Assert.AreEqual("a", nameAst.Value);
        }

        [Test]
        public void StatementSequenceWithSemicolon()
        {
            var statements = ParseInput("Set-Location ; Get-Location")
                    .EndBlock
                    .Statements;

            Assert.AreEqual(2, statements.Count);
        }

        [Test(Description = "Issue: https://github.com/JayBazuzi/Pash2/issues/7"), ExpectedException]
        public void StatementSequenceWithoutSemicolonTest()
        {
            var statements = ParseInput("if ($true) { } Get-Location")
                    .EndBlock
                    .Statements;

            Assert.AreEqual(2, statements.Count);
        }

        [Test]
        public void SingleStatementInBlockTest()
        {
            StringConstantExpressionAst command = ParseInput("{ Get-ChildItem }")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .ScriptBlock
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .CommandElements[0];

            Assert.AreEqual("Get-ChildItem", command.Value);
        }

        [TestFixture]
        public class VerbatimStringLiteralExpressionTests
        {
            StringConstantExpressionAst _stringConstantExpressionAst;
            CommandExpressionAst _commandExpressionAst;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._commandExpressionAst = ParseInput("'PS> '")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0];

                this._stringConstantExpressionAst = (StringConstantExpressionAst)this._commandExpressionAst.Expression;

            }

            [Test]
            public void StringConstantTypeTest()
            {
                Assert.AreEqual(StringConstantType.SingleQuoted, this._stringConstantExpressionAst.StringConstantType);
            }

            [Test]
            public void Value()
            {
                Assert.AreEqual("PS> ", this._stringConstantExpressionAst.Value);
            }

            [Test]
            public void StaticType()
            {
                Assert.AreEqual(typeof(string), this._stringConstantExpressionAst.StaticType);
            }

            [Test]
            public void Text()
            {
                Assert.AreEqual("'PS> '", this._stringConstantExpressionAst.Extent.Text);
            }

            [Test, Ignore("I don't know why this test fails & makes NUnit hang")]
            public void Parent()
            {
                Assert.AreEqual(this._commandExpressionAst, this._stringConstantExpressionAst.Parent);
            }
        }

        [TestFixture]
        public class IntegerLiteralExpressionTests
        {
            ConstantExpressionAst _constantExpressionAst;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._constantExpressionAst = ParseInput("1")
                    .EndBlock
                    .Statements[0]
                    .PipelineElements[0]
                    .Expression;
            }

            [Test]
            public void Value()
            {
                Assert.AreEqual(1, this._constantExpressionAst.Value);
            }

            [Test, Ignore]
            public void StaticType()
            {
                Assert.AreEqual(typeof(int), this._constantExpressionAst.StaticType);
            }

            [Test]
            public void Text()
            {
                Assert.AreEqual("1", this._constantExpressionAst.Extent.Text);
            }
        }

        [Test]
        public void HexIntegerLiteralTest()
        {
            int value = ParseInput("0xa")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .Value;
            Assert.AreEqual(0xA, value);
        }

        [Test]
        public void ExpandableStringLiteralExpression()
        {
            string value = ParseInput("\"PS> \"")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .Value;

            Assert.AreEqual("PS> ", value);
        }

        [Test]
        public void AdditiveExpression_AddStringInt()
        {
            var expression = ParseInput("'x' + 1")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression;

            ConstantExpressionAst leftValue = expression.Left;
            ConstantExpressionAst rightValue = expression.Right;

            Assert.AreEqual("x", leftValue.Value);
            Assert.AreEqual(TokenKind.Plus, expression.Operator);
            Assert.AreEqual(1, rightValue.Value);
        }

        [Test]
        public void NewlineContinuationTest()
        {
            var expression = ParseInput(
@"'x' + `
'y'"
                )
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression;
        }

        [Test]
        public void AdditiveExpression_AddStrings()
        {
            var expression = ParseInput("'x' + 'y'")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression;

            ConstantExpressionAst leftValue = expression.Left;
            ConstantExpressionAst rightValue = expression.Right;

            Assert.AreEqual("x", leftValue.Value);
            Assert.AreEqual(TokenKind.Plus, expression.Operator);
            Assert.AreEqual("y", rightValue.Value);
        }

        [Test]
        public void ParenthesizedExpression()
        {
            string result = ParseInput("(Get-Location)")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .Pipeline
                .PipelineElements[0]
                .CommandElements[0]
                .Value;

            Assert.AreEqual("Get-Location", result);
        }

        [Test]
        public void NegativeIntegerTest()
        {
            int result = ParseInput("-1")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .Value;

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void HexIntegerTest()
        {
            var result = ParseInput("0xA")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .Value;

            Assert.AreEqual(0xa, result);
        }

        [Test]
        public void ArrayRangeTest()
        {
            BinaryExpressionAst result = ParseInput("1..10")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression;

            ConstantExpressionAst left = (ConstantExpressionAst)result.Left;
            ConstantExpressionAst right = (ConstantExpressionAst)result.Right;

            Assert.AreEqual(1, left.Value);
            Assert.AreEqual(10, right.Value);
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
            var result = ParseInput("1,3,3")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                .Elements;

            Assert.AreEqual(1, result[0].Value);
            Assert.AreEqual(3, result[1].Value);
            Assert.AreEqual(3, result[2].Value);
        }

        [Test]
        public void PipelineTest()
        {
            var pipelineAst = ParseInput("x | y")
                .EndBlock
                .Statements[0];

            var firstCommand = pipelineAst.PipelineElements[0].CommandElements[0].Value;
            var secondCommand = pipelineAst.PipelineElements[1].CommandElements[0].Value;

            Assert.AreEqual("x", firstCommand);
            Assert.AreEqual("y", secondCommand);
        }

        static dynamic ParseInput(string s)
        {
            return PowerShellGrammar.ParseInteractiveInput(s);
        }
    }
}

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

        static dynamic ParseInput(string s)
        {
            return PowerShellGrammar.ParseInteractiveInput(s);
        }
    }
}

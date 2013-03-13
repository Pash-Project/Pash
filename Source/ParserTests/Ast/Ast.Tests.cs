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
using System.Diagnostics;

namespace ParserTests
{
    [TestFixture]
    public class AstTests
    {
        [Test, Description("I once wrote the `label` rule as as `foo:`, which broke this")]
        public void ScriptPathWithColon()
        {
            ParseInput(@"C:\foo.ps1");
        }

        [Test]
        public void IfEmptyStatementTest()
        {
            IfStatementAst ifStatementAst = ParseInput("if ($true) {}")
                .EndBlock
                .Statements[0]
                ;

            CollectionAssert.IsEmpty(ifStatementAst.Clauses.Single().Item2.Statements);
            Assert.IsNull(ifStatementAst.ElseClause);
        }

        [Test]
        public void IfWithStatementTest()
        {
            IfStatementAst ifStatementAst = ParseInput("if ($true) { Get-ChildItem }")
                .EndBlock
                .Statements[0]
                ;

            Assert.AreEqual(1, ifStatementAst.Clauses.Single().Item2.Statements.Count);
            Assert.IsNull(ifStatementAst.ElseClause);
        }

        [Test]
        public void IfElseTest()
        {
            IfStatementAst ifStatementAst = ParseInput("if ($true) {} else {}")
                .EndBlock
                .Statements[0]
                ;

            Assert.IsNotNull(ifStatementAst.ElseClause);
        }

        [Test]
        public void IfElseifTest()
        {
            IfStatementAst ifStatementAst = ParseInput("if ($true) {} elseif ($false) {}")
                .EndBlock
                .Statements[0]
                ;

            Assert.IsNull(ifStatementAst.ElseClause);
            Assert.AreEqual(2, ifStatementAst.Clauses.Count);
        }

        [Test]
        public void GreaterThatEqualTest()
        {
            var binaryExpressionAst = ParseInput("10 -gt 1")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                ;

            Assert.AreEqual(TokenKind.Igt, binaryExpressionAst.Operator);
            Assert.AreEqual(10, binaryExpressionAst.Left.Value);
            Assert.AreEqual(1, binaryExpressionAst.Right.Value);
        }

        [Test]
        public void CommentTest()
        {
            ScriptBlockAst scriptBlockAst = ParseInput(
@"
# a comment
9
");

            Assert.AreEqual(1, scriptBlockAst.EndBlock.Statements.Count);
        }

        [Test]
        public void MultiLineTest()
        {
            ScriptBlockAst scriptBlockAst = ParseInput(
@"
ls
ls
");
            Assert.AreEqual(2, scriptBlockAst.EndBlock.Statements.Count);
        }

        [Test]
        public void CommandInvocationOperatorTest()
        {
            var result = ParseInput("& 'ls'");
            Debug.WriteLine(result);
        }

        [Test]
        public void SemicolonOnly()
        {
            ScriptBlockAst scriptBlockAst = ParseInput(";");

            Assert.IsNull(scriptBlockAst.ParamBlock);
            Assert.IsNull(scriptBlockAst.BeginBlock);
            Assert.IsNull(scriptBlockAst.ProcessBlock);
            Assert.IsNull(scriptBlockAst.DynamicParamBlock);
            Assert.IsNull(scriptBlockAst.Parent);

            CollectionAssert.IsEmpty(scriptBlockAst.EndBlock.Statements);
        }

        [Test]
        public void ATest()
        {
            var scriptBlockAst = ParseInput("function F { 'hi' } ; F");

            FunctionDefinitionAst functionDefinitionAst = scriptBlockAst.EndBlock.Statements[0];
            PipelineAst pipelineAst = scriptBlockAst.EndBlock.Statements[1];

                                    
            Debug.WriteLine(functionDefinitionAst);
            Debug.WriteLine(pipelineAst);
        }

        [Test]
        public void TwoSemicolonsTest()
        {
            ParseInput("a ; ; b");
        }

        [Test]
        public void SemicolonTerminatedTest()
        {
            ScriptBlockAst scriptBlockAst = ParseInput("Get-ChildItem;");

            Assert.IsNull(scriptBlockAst.ParamBlock);
            Assert.IsNull(scriptBlockAst.BeginBlock);
            Assert.IsNull(scriptBlockAst.ProcessBlock);
            Assert.IsNull(scriptBlockAst.DynamicParamBlock);
            Assert.IsNull(scriptBlockAst.Parent);

            Assert.AreEqual(1, scriptBlockAst.EndBlock.Statements.Count);
        }

        [Test]
        public void FunctionTest()
        {
            FunctionDefinitionAst functionDefinitionAst = ParseInput("function f { 'x' }").
                   EndBlock.
                   Statements[0];

            Assert.IsFalse(functionDefinitionAst.IsFilter);
            Assert.IsFalse(functionDefinitionAst.IsWorkflow);
            Assert.AreEqual("f", functionDefinitionAst.Name);
        }

        [Test]
        public void AssignmentTest()
        {
            AssignmentStatementAst assignmentStatementAst = ParseInput("$x = 'y'").
                    EndBlock.
                    Statements[0];

            Assert.AreEqual(TokenKind.Equals, assignmentStatementAst.Operator);
        }

        [TestFixture]
        public class VariableExpressionAstTests
        {
            [Test]
            public void Simple()
            {
                VariableExpressionAst variableExpressionAst = ParseInput("$x").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression;

                Assert.False(variableExpressionAst.Splatted);
                Assert.AreEqual(typeof(object), variableExpressionAst.StaticType);

                var variablePath = variableExpressionAst.VariablePath;

                Assert.AreEqual("x", variablePath.UserPath);
                Assert.False(variablePath.IsGlobal);
                Assert.False(variablePath.IsLocal);
                Assert.False(variablePath.IsPrivate);
                Assert.False(variablePath.IsScript);
                Assert.True(variablePath.IsUnqualified);
                Assert.True(variablePath.IsUnscopedVariable);
                Assert.True(variablePath.IsVariable);
                Assert.False(variablePath.IsDriveQualified);
                Assert.IsNull(variablePath.DriveName);
            }

            [Test]
            public void Global()
            {
                VariablePath variablePath = ParseInput("$global:x").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("global:x", variablePath.UserPath);
                Assert.True(variablePath.IsGlobal);
                Assert.False(variablePath.IsLocal);
                Assert.False(variablePath.IsPrivate);
                Assert.False(variablePath.IsScript);
                Assert.False(variablePath.IsUnqualified);
                Assert.False(variablePath.IsUnscopedVariable);
                Assert.True(variablePath.IsVariable);
                Assert.False(variablePath.IsDriveQualified);
                Assert.IsNull(variablePath.DriveName);
            }

            [Test]
            public void Function()
            {
                VariablePath variablePath = ParseInput("$function:prompt").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("function:prompt", variablePath.UserPath);
                Assert.False(variablePath.IsGlobal);
                Assert.False(variablePath.IsLocal);
                Assert.False(variablePath.IsPrivate);
                Assert.False(variablePath.IsScript);
                Assert.False(variablePath.IsUnqualified);
                Assert.False(variablePath.IsUnscopedVariable);
                Assert.False(variablePath.IsVariable);
                Assert.True(variablePath.IsDriveQualified);
                Assert.AreEqual("function", variablePath.DriveName);
            }

            [Test]
            public void Local()
            {
                VariablePath variablePath = ParseInput("$local:x").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("local:x", variablePath.UserPath);
                Assert.False(variablePath.IsGlobal);
                Assert.True(variablePath.IsLocal);
                Assert.False(variablePath.IsPrivate);
                Assert.False(variablePath.IsScript);
                Assert.False(variablePath.IsUnqualified);
                Assert.False(variablePath.IsUnscopedVariable);
                Assert.True(variablePath.IsVariable);
                Assert.False(variablePath.IsDriveQualified);
                Assert.IsNull(variablePath.DriveName);
            }

            [Test, Explicit]
            public void Dollar()
            {
                VariablePath variablePath = ParseInput("$$").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("$", variablePath.UserPath);
                Assert.False(variablePath.IsGlobal);
                Assert.False(variablePath.IsLocal);
                Assert.False(variablePath.IsPrivate);
                Assert.False(variablePath.IsScript);
                Assert.True(variablePath.IsUnqualified);
                Assert.True(variablePath.IsUnscopedVariable);
                Assert.True(variablePath.IsVariable);
                Assert.False(variablePath.IsDriveQualified);
                Assert.IsNull(variablePath.DriveName);
            }
        }

        [TestFixture, Explicit]
        public class ScriptBlockTests
        {
            [Test, Explicit]
            public void Empty()
            {
                ScriptBlockAst scriptBlockAst = ParseInput("{}").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0].
                    Expression.
                    ScriptBlock;

                Assert.AreEqual(0, scriptBlockAst.ParamBlock.Parameters.Count);
                Assert.AreEqual(0, scriptBlockAst.EndBlock.Statements.Count);
            }

            [Test, Explicit]
            public void Param()
            {
                ScriptBlockAst scriptBlockAst = ParseInput("{ param ([string]$s) }").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0].
                    Expression.
                    ScriptBlock;

                Assert.AreEqual(1, scriptBlockAst.ParamBlock.Parameters.Count);
                Assert.AreEqual(0, scriptBlockAst.EndBlock.Statements.Count);
            }

            [Test, Explicit]
            public void Statement()
            {
                ScriptBlockAst scriptBlockAst = ParseInput("{ Get-ChildItem }").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0].
                    Expression.
                    ScriptBlock;

                Assert.AreEqual(0, scriptBlockAst.ParamBlock.Parameters.Count);
                Assert.AreEqual(1, scriptBlockAst.EndBlock.Statements.Count);
            }

            [Test, Explicit]
            public void ParamAndStatement()
            {
                ScriptBlockAst scriptBlockAst = ParseInput("{ param ([string]$s) Get-ChildItem }").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0].
                    Expression.
                    ScriptBlock;

                Assert.AreEqual(1, scriptBlockAst.ParamBlock.Parameters.Count);
                Assert.AreEqual(1, scriptBlockAst.EndBlock.Statements.Count);
            }
        }

        [Test]
        public void IndexTest()
        {
            var indexExpressionAst = ParseInput("'abc'[2]")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression
                ;

            Assert.AreEqual("abc", indexExpressionAst.Target.Value);
            Assert.AreEqual(2, indexExpressionAst.Index.Value);
        }

        [Test
            // This is a bug, but not a serious one.
            //, ExpectedException(typeof(PowerShellGrammar.ParseException))
        ]
        public void IndexWithSpaceShouldFail()
        {
            ParseInput("'abc' [2]");
        }

        [Test, Explicit]
        public void MemberAccess()
        {
            MemberExpressionAst memberExpressionAst = ParseInput("[System.Int32]::MaxValue")
                .EndBlock
                .Statements[0]
                .PipelineElements[0]
                .Expression;

            Debug.WriteLine(memberExpressionAst);
        }

        [Test]
        public void BadMemberAccess()
        {
            Assert.Throws<PowerShellGrammar.ParseException>(() => {

                // The language spec says this space is prohibited.
                ParseInput("[System.Int32] ::MaxValue");

            });
        }

        [Test]
        public void ParseError()
        {
            Assert.Throws<PowerShellGrammar.ParseException>(() => {
                
                ParseInput("$");

            });
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

        [Test(Description = "Issue: https://github.com/JayBazuzi/Pash2/issues/7")]
        public void StatementSequenceWithoutSemicolonTest()
        {
            Assert.Throws<PowerShellGrammar.ParseException>(() => {

                var statements = ParseInput("if ($true) { } Get-Location")
                    .EndBlock
                        .Statements;
                
                Assert.AreEqual(2, statements.Count);

            });
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

            [Test, Explicit("I don't know why this test fails & makes NUnit hang")]
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

            [Test]
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

        [Test, Explicit("disabled because it requires an extension to Irony that is incompatable with Irony's GrammarExplorer")]
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

         	Debug.WriteLine(expression);
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

        [Test, Explicit]
        public void UnaryMinusTest()
        {
            ////    7.2.5 Unary minus
            // TODO:
            ////    Examples:
            ////    
            ////    -$true         # type int, value -1
            ////    -123L          # type long, value -123
            ////    -0.12340D      # type decimal, value -0.12340
            Assert.Fail("test not yet implemented");
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

        [Test]
        public void Pipeline3Test()
        {
            var pipelineAst = ParseInput("x | y | z")
                .EndBlock
                .Statements[0];

            var firstCommand = pipelineAst.PipelineElements[0].CommandElements[0].Value;
            var secondCommand = pipelineAst.PipelineElements[1].CommandElements[0].Value;
            var thirdCommand = pipelineAst.PipelineElements[2].CommandElements[0].Value;

            Assert.AreEqual("x", firstCommand);
            Assert.AreEqual("y", secondCommand);
            Assert.AreEqual("z", thirdCommand);
        }

        static dynamic ParseInput(string s)
        {
            return PowerShellGrammar.ParseInteractiveInput(s);
        }
    }
}

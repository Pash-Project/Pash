// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using Pash.ParserIntrinsics;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.IO;

namespace ParserTests
{
    [TestFixture]
    public class AstTests
    {
        [Test]
        public void ConfigScriptIsValid()
        {
            var result = ParseInput(File.ReadAllText("config.ps1"));
        }

        [Test, Description("Did this tokenize as 1 long string?")]
        public void TwoStrings()
        {
            BinaryExpressionAst expressionAst = ParseStatement(@"""a"" + ""b""")
                .PipelineElements[0]
                .Expression
                ;

            Assert.AreEqual(TokenKind.Plus, expressionAst.Operator);
        }

        [Test, Description("I once wrote the `label` rule as as `foo:`, which broke this")]
        public void ScriptPathWithColon()
        {
            ParseInput(@"C:\foo.ps1");
        }

        [TestFixture]
        public class ConditionalTests
        {
            [Test]
            public void IfEmptyStatementTest()
            {
                IfStatementAst ifStatementAst = ParseStatement("if ($true) {}");

                CollectionAssert.IsEmpty(ifStatementAst.Clauses.Single().Item2.Statements);
                Assert.IsNull(ifStatementAst.ElseClause);
            }

            [Test]
            public void IfWithStatementTest()
            {
                IfStatementAst ifStatementAst = ParseStatement("if ($true) { Get-ChildItem }");

                Assert.AreEqual(1, ifStatementAst.Clauses.Single().Item2.Statements.Count);
                Assert.IsNull(ifStatementAst.ElseClause);
            }

            [Test]
            public void IfElseTest()
            {
                IfStatementAst ifStatementAst = ParseStatement("if ($true) {} else {}");

                Assert.IsNotNull(ifStatementAst.ElseClause);
            }

            [Test]
            public void IfElseifTest()
            {
                IfStatementAst ifStatementAst = ParseStatement("if ($true) {} elseif ($false) {}");

                Assert.IsNull(ifStatementAst.ElseClause);
                Assert.AreEqual(2, ifStatementAst.Clauses.Count);
            }
        }

        [Test]
        public void GreaterThatEqualTest()
        {
            var binaryExpressionAst = ParseStatement("10 -gt 1")
                .PipelineElements[0]
                .Expression
                ;

            Assert.AreEqual(TokenKind.Igt, binaryExpressionAst.Operator);
            Assert.AreEqual(10, binaryExpressionAst.Left.Value);
            Assert.AreEqual(1, binaryExpressionAst.Right.Value);
        }

        [Test]
        [TestCase("# a comment\n9")]
        [TestCase("9\n# a comment")]
        [TestCase("9 # a comment")]
        public void SingleLineCommentTestSimple(string input)
        {
            ScriptBlockAst scriptBlockAst = ParseInput(input);

            Assert.AreEqual(1, scriptBlockAst.EndBlock.Statements.Count);
            Assert.AreEqual("9", scriptBlockAst.EndBlock.Statements[0].ToString());
        }

        [Test, Combinatorial]
        public void CommentTestCommentAdjacentToToken(
            [Values("'a'", "\"a\"", "(ls)", "$a[1]", "$a", "1;", "&{gci}", "1" /* , "1.", "1.0", ".2", "[int]" */)]
            string statement,
            [Values("#foo" /*, "<# foo #>" */)]
            string comment)
        {
            // “A character sequence is only recognized as a comment if that
            // sequence begins with # or <#. For example, hello#there is
            // considered a single token whereas hello #there is considered the
            // token hello followed by a single-line comment. As well as following
            // white space, the comment start sequence can also be preceded by any
            // expression-terminating or statement-terminating character
            // (such as ), }, ], ', ", or ;).”

            // Furthermore:
            // “A token is the smallest lexical element within the PowerShell
            // language. Tokens can be separated by new-lines, comments, white
            // space, or any combination thereof.”

            // This means that 42#foo is a valid statement (42) and a comment
            // because the comment terminates the token (it doesn't for a#b,
            // though).

            ScriptBlockAst scriptBlockAst = ParseInput(statement + comment);

            Assert.AreEqual(1, scriptBlockAst.EndBlock.Statements.Count);
            // If the comment would have been parsed as part of the statement
            // the `#` would appear here. It shouldn't.
            StringAssert.DoesNotContain("#", scriptBlockAst.EndBlock.Statements[0].ToString());
        }

        [Test]
        [TestCase("<##>9")]
        [TestCase("<##> 9")]
        [TestCase("9 <##>")]
        [TestCase("<# a comment #> 9")]
        [TestCase("9 <# a comment #>")]
        [TestCase("<# a \n comment #> 9")]
        [TestCase("9 <# a \n comment #>")]
        public void DelimitedCommentTest(string input)
        {
            ScriptBlockAst scriptBlockAst = ParseInput(input);

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
        public void FunctionWithOneParameter()
        {
            FunctionDefinitionAst functionDefinitionAst = ParseInput("function Update-File($file) { 'x' }").
                   EndBlock.
                   Statements[0];

            Assert.AreEqual("Update-File", functionDefinitionAst.Name);
            Assert.AreEqual(1, functionDefinitionAst.Parameters.Count);
            Assert.AreEqual("file", functionDefinitionAst.Parameters[0].Name.VariablePath.UserPath);
        }

        [Test]
        public void FunctionWithTwoParameters()
        {
            FunctionDefinitionAst functionDefinitionAst = ParseInput("function Update-File($param1, $param2) { 'x' }").
                   EndBlock.
                   Statements[0];

            Assert.AreEqual("Update-File", functionDefinitionAst.Name);
            Assert.AreEqual(2, functionDefinitionAst.Parameters.Count);
            Assert.AreEqual("param1", functionDefinitionAst.Parameters[0].Name.VariablePath.UserPath);
            Assert.AreEqual("param2", functionDefinitionAst.Parameters[1].Name.VariablePath.UserPath);
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
            [TestCase("x", "Latin")]
            [TestCase("Ⴉ", "Category Lu")]
            [TestCase("ζ", "Category Ll")]
            [TestCase("ᾮ", "Category Lt")]
            [TestCase("ʱ", "Category Lm")]
            [TestCase("מ", "Category Lo")]
            [TestCase("1", "Category Nd, arabic numeral")]
            [TestCase("७", "Category Nd, other Unicode numeral")]
            public void Simple(string variableName, string message)
            {
                VariableExpressionAst variableExpressionAst = ParseInput("$" + variableName).
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression;

                Assert.False(variableExpressionAst.Splatted);
                Assert.AreEqual(typeof(object), variableExpressionAst.StaticType);

                var variablePath = variableExpressionAst.VariablePath;

                Assert.AreEqual(variableName, variablePath.UserPath, message);
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

            [Test, Explicit]
            public void Caret()
            {
                VariablePath variablePath = ParseInput("$^").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("^", variablePath.UserPath);
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

            [Test, Explicit]
            public void QuestionMark()
            {
                VariablePath variablePath = ParseInput("$?").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("?", variablePath.UserPath);
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
            public void Underscore()
            {
                VariablePath variablePath = ParseInput("$_").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("_", variablePath.UserPath);
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

            [Test, Explicit]
            public void BracedVariableSimple()
            {
                VariablePath variablePath = ParseInput("${a}").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("a", variablePath.UserPath);
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

            [Test, Explicit]
            public void BracedVariableArbitraryUnicode()
            {
                VariablePath variablePath = ParseInput("${➠▦⍥}").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual("➠▦⍥", variablePath.UserPath);
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

            [Test, Explicit]
            public void BracedVariablePath()
            {
                VariablePath variablePath = ParseInput(@"${E:\File.txt}").
                    EndBlock.
                    Statements[0].
                    PipelineElements[0]
                    .Expression
                    .VariablePath;

                Assert.AreEqual(@"E:\File.txt", variablePath.UserPath);
                Assert.False(variablePath.IsGlobal);
                Assert.False(variablePath.IsLocal);
                Assert.False(variablePath.IsPrivate);
                Assert.False(variablePath.IsScript);
                Assert.True(variablePath.IsUnqualified);
                Assert.True(variablePath.IsUnscopedVariable);
                Assert.False(variablePath.IsVariable);
                Assert.True(variablePath.IsDriveQualified);
                Assert.AreEqual("E:", variablePath.DriveName);
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
            var indexExpressionAst = ParseStatement("'abc'[2]")
                .PipelineElements[0]
                .Expression
                ;

            Assert.AreEqual("abc", indexExpressionAst.Target.Value);
            Assert.AreEqual(2, indexExpressionAst.Index.Value);
        }

        [Test, ExpectedException(typeof(PowerShellGrammar.ParseException))
        ]
        public void IndexWithSpaceShouldFail()
        {
            ParseInput("'abc' [2]");
        }

        [Test]
        public void ParseError()
        {
            Assert.Throws<PowerShellGrammar.ParseException>(() =>
            {

                ParseInput("$");

            });
        }

        [Test]
        public void HashTable0()
        {
            HashtableAst hashtableAst = ParseStatement("@{ }")
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(0, hashtableAst.KeyValuePairs.Count);
        }

        [Test]
        public void HashTable1()
        {
            HashtableAst hashtableAst = ParseStatement("@{ 'a' = 'b' }")
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
        [TestCase("@{ a = b ; c = d }", "Key-Value pairs separated by semicolon")]
        [TestCase("@{ a = b \n c = d }", "Key-Value pairs separated by line break")]
        public void HashTable2(string input, string message)
        {
            // hash-entry:
            //     key-expression = new-lines_opt statement
            // statement-terminator:
            //     ;
            //     new-line-character

            HashtableAst hashtableAst = ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(2, hashtableAst.KeyValuePairs.Count, message);
        }

        [Test]
        [TestCase("@{\n\n}", 0)]
        [TestCase("@{\r\n}", 0)]
        [TestCase("@{\r\r}", 0)]
        [TestCase("@{\na = b}", 1)]
        [TestCase("@{\ra = b}", 1)]
        [TestCase("@{\r\na = b}", 1)]
        [TestCase("@{a = b\n}", 1)]
        [TestCase("@{a = b\r}", 1)]
        [TestCase("@{a = b\r\n}", 1)]
        [TestCase("@{\na = b\n}", 1)]
        [TestCase("@{\ra = b\n}", 1)]
        [TestCase("@{\na = b\r}", 1)]
        [TestCase("@{\r\na = b\n}", 1)]
        [TestCase("@{\na = b\r\n}", 1)]
        [TestCase("@{\na = b\nb = c}", 2)]
        [TestCase("@{a = b\nb = c\n}", 2)]
        public void HashTableAcceptsLeadingAndTrailingLineBreaks(string input, int expectedCount)
        {
            // hash-literal-expression:
            //     @{ new-lines_opt hash-literal-body_opt new-lines_opt }            

            HashtableAst hashtableAst = ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(expectedCount, hashtableAst.KeyValuePairs.Count);
        }

        [Test]
        [TestCase("@{a=\nb}")]
        [TestCase("@{a=\rb}")]
        [TestCase("@{a=\r\nb}")]
        [TestCase("@{a=\n\nb}")]
        [TestCase("@{a=\r\rb}")]
        [TestCase("@{a=\r\n\r\nb}")]
        public void HashTableAcceptsLineBreaksAfterEquals(string input)
        {
            // hash-entry:
            //     key-expression = new-lines_opt statement

            HashtableAst hashtableAst = ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(1, hashtableAst.KeyValuePairs.Count);
        }

        [Test]
        [TestCase("@{a = b ; ; ; }", 1)]
        [TestCase("@{a = b ; \n ; }", 1)]
        [TestCase("@{a = b ; \r\n ; }", 1)]
        [TestCase("@{a = b ; ; ; \r\n ; ; ; }", 1)]
        [TestCase("@{a = b ; ; ; b = c}", 2)]
        [TestCase("@{a = b ;\r\n b = c}", 2)]
        [TestCase("@{a = b ;\n b = c}", 2)]
        [TestCase("@{a = b ;\r\n ; ; b = c}", 2)]
        public void HashTableAcceptsMultipleStatementTerminators(string input, int expectedCount)
        {
            // statement-terminators:
            //     statement-terminator
            //     statement-terminators statement-terminator
            // statement-terminator:
            //     ;
            //     new-line-character

            HashtableAst hashtableAst = ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;

            Assert.AreEqual(expectedCount, hashtableAst.KeyValuePairs.Count);
        }

        [Test]
        public void HashTableIntegerKey()
        {
            HashtableAst hashtableAst = ParseStatement("@{ 10 = 'b' }")
                    .PipelineElements[0]
                    .Expression;

            var nameAst = (ConstantExpressionAst)hashtableAst.KeyValuePairs.Single().Item1;

            Assert.AreEqual(10, nameAst.Value);
        }

        [Test]
        public void HashTableUnquotedName()
        {
            HashtableAst hashtableAst = ParseStatement("@{ a = 'b' }")
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

        [Test(Description = "Issue: https://github.com/Pash-Project/Pash/issues/7")]
        public void StatementSequenceWithoutSemicolonTest()
        {
            Assert.Throws<PowerShellGrammar.ParseException>(() =>
            {

                var statements = ParseInput("if ($true) { } Get-Location")
                    .EndBlock
                        .Statements;

                Assert.AreEqual(2, statements.Count);

            });
        }

        [Test]
        public void SingleStatementInBlockTest()
        {
            StringConstantExpressionAst command = ParseStatement("{ Get-ChildItem }")
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
                this._commandExpressionAst = ParseStatement("'PS> '")
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
                this._constantExpressionAst = ParseStatement("1")
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
            int value = ParseStatement("0xa")
                .PipelineElements[0]
                .Expression
                .Value;
            Assert.AreEqual(0xA, value);
        }

        [Test]
        public void ExpandableStringLiteralExpression()
        {
            string value = ParseStatement("\"PS> \"")
                .PipelineElements[0]
                .Expression
                .Value;

            Assert.AreEqual("PS> ", value);
        }

        [Test]
        public void AdditiveExpression_AddStringInt()
        {
            var expression = ParseStatement("'x' + 1")
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
            var expression = ParseStatement(
@"'x' + `
'y'"
                )
                .PipelineElements[0]
                .Expression;
        }

        [Test]
        public void AdditiveExpression_AddStrings()
        {
            var expression = ParseStatement("'x' + 'y'")
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
            string result = ParseStatement("(Get-Location)")
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
            int result = ParseStatement("-1")
                .PipelineElements[0]
                .Expression
                .Value;

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void HexIntegerTest()
        {
            var result = ParseStatement("0xA")
                .PipelineElements[0]
                .Expression
                .Value;

            Assert.AreEqual(0xa, result);
        }

        [Test]
        public void ArrayRangeTest()
        {
            BinaryExpressionAst result = ParseStatement("1..10")
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
            var result = ParseStatement("1,3,3")
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
            var pipelineAst = ParseStatement("x | y");

            var firstCommand = pipelineAst.PipelineElements[0].CommandElements[0].Value;
            var secondCommand = pipelineAst.PipelineElements[1].CommandElements[0].Value;

            Assert.AreEqual("x", firstCommand);
            Assert.AreEqual("y", secondCommand);
        }

        [Test]
        public void Pipeline3Test()
        {
            var pipelineAst = ParseStatement("x | y | z");

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

        static dynamic ParseStatement(string input)
        {
            return ParseInput(input)
                .EndBlock
                .Statements[0];
        }

        [TestFixture]
        public class MemberAccess
        {
            [Test]
            public void StaticProperty()
            {
                MemberExpressionAst memberExpressionAst = ParseStatement("[System.Int32]::MaxValue")
                    .PipelineElements[0]
                    .Expression;
            }

            [Test]
            public void InstanceProperty()
            {
                var memberExpressionAst = ParseStatement(@"'abc'.Length")
                    .PipelineElements[0]
                    .Expression;
                StringConstantExpressionAst expressionAst = memberExpressionAst.Expression;
                StringConstantExpressionAst memberAst = memberExpressionAst.Member;

                Assert.AreEqual("abc", expressionAst.Value);
                Assert.AreEqual(StringConstantType.SingleQuoted, expressionAst.StringConstantType);

                Assert.AreEqual("Length", memberAst.Value);
                Assert.AreEqual(StringConstantType.BareWord, memberAst.StringConstantType);
            }

            [Test]
            public void BadMemberAccess()
            {
                Assert.Throws<PowerShellGrammar.ParseException>(() =>
                {

                    // The language spec says this space is prohibited.
                    ParseInput("[System.Int32] ::MaxValue");

                });
            }

            [Test]
            public void StaticMethodInvocation()
            {
                InvokeMemberExpressionAst invokeMemberExpressionAst = ParseStatement(@"[char]::IsUpper('a')")
                    .PipelineElements[0]
                    .Expression;

                var typeExpressionAst = (TypeExpressionAst)invokeMemberExpressionAst.Expression;
                var stringConstantExpressionAst = (StringConstantExpressionAst)invokeMemberExpressionAst.Member;
                var argumentAst = (StringConstantExpressionAst)invokeMemberExpressionAst.Arguments.Single();

                Assert.AreEqual("char", typeExpressionAst.TypeName.Name);

                Assert.AreEqual("IsUpper", stringConstantExpressionAst.Value);

                Assert.AreEqual("a", argumentAst.Value);
            }
        }

        [TestFixture]
        public class LineContinuationTests
        {
            [Test]
            public void CarriageReturn()
            {
                IEnumerable<StatementAst> statements = ParseInput("Write-Host`\r'xxx'")
                    .EndBlock
                    .Statements;

                Assert.AreEqual(1, statements.Count());
            }

            [Test]
            public void LineFeed()
            {
                IEnumerable<StatementAst> statements = ParseInput("Write-Host`\n'xxx'")
                    .EndBlock
                    .Statements;

                Assert.AreEqual(1, statements.Count());
            }

            [Test]
            public void CarriageReturnLineFeed()
            {
                IEnumerable<StatementAst> statements = ParseInput("Write-Host`\r\n'xxx'")
                    .EndBlock
                    .Statements;

                Assert.AreEqual(1, statements.Count());
            }

            [Test]
            public void CarriageReturnCarriageReturn()
            {
                IEnumerable<StatementAst> statements = ParseInput("Write-Host`\r\r'xxx'")
                    .EndBlock
                    .Statements;

                Assert.AreEqual(2, statements.Count());
            }

            [Test]
            public void LineFeedLineFeed()
            {
                IEnumerable<StatementAst> statements = ParseInput("Write-Host`\n\n'xxx'")
                    .EndBlock
                    .Statements;

                Assert.AreEqual(2, statements.Count());
            }
        }

        [Test]
        public void ArrayArgument()
        {
            var commandElementAsts = ParseStatement("Write-Host 3,$true")
                .PipelineElements[0]
                .CommandElements;

            StringConstantExpressionAst stringConstantExpressionAst = commandElementAsts[0];

            Assert.AreEqual("Write-Host", stringConstantExpressionAst.Value);

            var expressionAst = commandElementAsts[1];

            ConstantExpressionAst value0 = expressionAst.Elements[0];
            Assert.AreEqual(3, value0.Value);

            VariableExpressionAst value1 = expressionAst.Elements[1];
            Assert.AreEqual("true", value1.VariablePath.UserPath);
        }

        [Test]
        public void LogicalOperator()
        {
            BinaryExpressionAst binaryExpressionAst = ParseStatement("($true) -or ($false)")
                .PipelineElements[0]
                .Expression;

            Assert.AreEqual(TokenKind.Or, binaryExpressionAst.Operator);
        }

        [Test]
        public void NotOperator()
        {
            UnaryExpressionAst unaryExpressionAst = ParseStatement("-not $true")
                .PipelineElements[0]
                .Expression;

            Assert.AreEqual(TokenKind.Not, unaryExpressionAst.TokenKind);
            VariableExpressionAst variableExpressionAst = (VariableExpressionAst)unaryExpressionAst.Child;
            Assert.AreEqual("true", variableExpressionAst.VariablePath.UserPath);
        }

        [Test]
        public void PostIncrementExpression()
        {
            UnaryExpressionAst unaryExpressionAst = ParseStatement("$x++")
                .PipelineElements[0]
                .Expression;

            Assert.AreEqual(TokenKind.PostfixPlusPlus, unaryExpressionAst.TokenKind);

            VariableExpressionAst variableExpressionAst = (VariableExpressionAst)unaryExpressionAst.Child;
            Assert.AreEqual("x", variableExpressionAst.VariablePath.UserPath);
        }

        [Test]
        public void Return()
        {
            ReturnStatementAst returnStatementAst = ParseStatement("{ return }")
                .PipelineElements[0]
                .Expression
                .ScriptBlock
                .EndBlock
                .Statements[0];

            Assert.Null(returnStatementAst.Pipeline);
        }

        [Test]
        public void Exit()
        {
            ExitStatementAst exitStatementAst = ParseStatement("{ exit }")
                .PipelineElements[0]
                .Expression
                .ScriptBlock
                .EndBlock
                .Statements[0];

            Assert.Null(exitStatementAst.Pipeline);
        }

        [Test]
        public void Cast()
        {
            ConvertExpressionAst convertExpressionAst = ParseStatement("[Text.RegularExpressions.RegexOptions] 'IgnoreCase'")
                .PipelineElements[0]
                .Expression;

            Assert.AreEqual("Text.RegularExpressions.RegexOptions", convertExpressionAst.Type.TypeName.Name);
            Assert.AreEqual("IgnoreCase", ((StringConstantExpressionAst)convertExpressionAst.Child).Value);
        }

        [Test]
        public void For()
        {
            ForStatementAst forStatementAst = ParseStatement("for ($i = 0; $i -ile 10; $i++) {Write-Host $i}");
        }

        [Test]
        public void ForEach()
        {
            ForEachStatementAst foreachStatementAst = ParseStatement("foreach ($i in (0..10)) {Write-Host $i}");

            Assert.AreEqual("i", foreachStatementAst.Variable.VariablePath.UserPath);
        }

        [Test]
        public void ArraySubexpression()
        {
            ArrayExpressionAst arrayExpressionAst = ParseStatement("@(1)").PipelineElements[0].Expression;

            Assert.AreEqual(1, arrayExpressionAst.SubExpression.Statements.Count);
        }

        [Test]
        public void ParamBlockWithOneParameterTest()
        {
            ParamBlockAst result = ParseInput("param($path)")
                .ParamBlock;

            ParameterAst parameter = result.Parameters.FirstOrDefault();
            Assert.AreEqual(1, result.Parameters.Count);
            Assert.AreEqual("path", parameter.Name.VariablePath.UserPath);
        }

        [Test]
        public void ParamBlockWithTwoParametersTest()
        {
            ParamBlockAst result = ParseInput("param($first, $second)")
                .ParamBlock;

            ParameterAst firstParameter = result.Parameters.FirstOrDefault();
            ParameterAst secondParameter = result.Parameters.LastOrDefault();
            Assert.AreEqual(2, result.Parameters.Count);
            Assert.AreEqual("first", firstParameter.Name.VariablePath.UserPath);
            Assert.AreEqual("second", secondParameter.Name.VariablePath.UserPath);
        }
        
        [Test]
        public void ParamBlockWithOneParameterWithDefaultIntegerValueTest()
        {
            ParamBlockAst result = ParseInput("param($first = 2)")
                .ParamBlock;

            ParameterAst parameter = result.Parameters.FirstOrDefault();
            var constantValue = (ConstantExpressionAst)parameter.DefaultValue;
            Assert.AreEqual(2, constantValue.Value);
        }

        [TestFixture]
        public class TryCatchTests
        {
            [Test]
            public void TryCatchAll()
            {
                TryStatementAst tryStatementAst = ParseStatement("try { } catch { }");

                Assert.AreEqual(1, tryStatementAst.CatchClauses.Count);
                CollectionAssert.IsEmpty(tryStatementAst.Body.Statements);
                CollectionAssert.IsEmpty(tryStatementAst.CatchClauses.Single().Body.Statements);
            }
            
            [Test]
            public void TryCatchWithSingleStatements()
            {
                TryStatementAst tryStatementAst = ParseStatement("try { Get-ChildItem } catch { Write-Host 'failed' }");

                Assert.AreEqual(1, tryStatementAst.Body.Statements.Count);
                Assert.AreEqual(1, tryStatementAst.CatchClauses.Single().Body.Statements.Count);
            }
        }

        [Test]
        public void AssignmentByAdditionOperator()
        {
            AssignmentStatementAst assignmentStatementAst = ParseStatement("$i += 10");

            var variableAst = (VariableExpressionAst)assignmentStatementAst.Left;
            var commandAst = (CommandExpressionAst)assignmentStatementAst.Right.Children.First();
            var constantAst = (ConstantExpressionAst)commandAst.Expression;
            Assert.AreEqual("i", variableAst.VariablePath.UserPath);
            Assert.AreEqual(TokenKind.PlusEquals, assignmentStatementAst.Operator);
            Assert.AreEqual(10, constantAst.Value);
        }

        [Test]
        public void AssignmentBySubtractionOperator()
        {
            AssignmentStatementAst assignmentStatementAst = ParseStatement("$i -= 10");

            var variableAst = (VariableExpressionAst)assignmentStatementAst.Left;
            var commandAst = (CommandExpressionAst)assignmentStatementAst.Right.Children.First();
            var constantAst = (ConstantExpressionAst)commandAst.Expression;
            Assert.AreEqual("i", variableAst.VariablePath.UserPath);
            Assert.AreEqual(TokenKind.MinusEquals, assignmentStatementAst.Operator);
            Assert.AreEqual(10, constantAst.Value);
        }
        
        [Test]
        public void AssignmentByMultiplicationOperator()
        {
            AssignmentStatementAst assignmentStatementAst = ParseStatement("$i *= 10");

            var variableAst = (VariableExpressionAst)assignmentStatementAst.Left;
            var commandAst = (CommandExpressionAst)assignmentStatementAst.Right.Children.First();
            var constantAst = (ConstantExpressionAst)commandAst.Expression;
            Assert.AreEqual("i", variableAst.VariablePath.UserPath);
            Assert.AreEqual(TokenKind.MultiplyEquals, assignmentStatementAst.Operator);
            Assert.AreEqual(10, constantAst.Value);
        }

        [Test]
        public void AssignmentByDivisionOperator()
        {
            AssignmentStatementAst assignmentStatementAst = ParseStatement("$i /= 2");

            var variableAst = (VariableExpressionAst)assignmentStatementAst.Left;
            var commandAst = (CommandExpressionAst)assignmentStatementAst.Right.Children.First();
            var constantAst = (ConstantExpressionAst)commandAst.Expression;
            Assert.AreEqual("i", variableAst.VariablePath.UserPath);
            Assert.AreEqual(TokenKind.DivideEquals, assignmentStatementAst.Operator);
            Assert.AreEqual(2, constantAst.Value);
        }

        [Test]
        public void AssignmentByModulusOperator()
        {
            AssignmentStatementAst assignmentStatementAst = ParseStatement("$i %= 4");

            var variableAst = (VariableExpressionAst)assignmentStatementAst.Left;
            var commandAst = (CommandExpressionAst)assignmentStatementAst.Right.Children.First();
            var constantAst = (ConstantExpressionAst)commandAst.Expression;
            Assert.AreEqual("i", variableAst.VariablePath.UserPath);
            Assert.AreEqual(TokenKind.RemainderEquals, assignmentStatementAst.Operator);
            Assert.AreEqual(4, constantAst.Value);
        }
    }
}

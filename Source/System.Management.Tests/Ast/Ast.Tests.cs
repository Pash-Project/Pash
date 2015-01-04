// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pash.ParserIntrinsics;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.IO;
using Pash.Implementation;

namespace ParserTests
{
    [TestFixture]
    public class AstTests
    {
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
            Assert.DoesNotThrow(delegate {
                ParseInput("& 'ls'");
            });
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
            Assert.AreEqual(2, scriptBlockAst.EndBlock.Statements.Count);
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
        public void FunctionWithEmptyParameterListTest()
        {
            FunctionDefinitionAst functionDefinitionAst = ParseInput("function f() { 'x' }").
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

            [Test]
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

        [Test, ExpectedException(typeof(ParseException))
        ]
        public void IndexWithSpaceShouldFail()
        {
            ParseInput("'abc' [2]");
        }

        [Test]
        public void ParseError()
        {
            Assert.Throws<ParseException>(() =>
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
        public void EmptyStatementList()
        {
            var statements = ParseInput("")
                .EndBlock
                .Statements;

            Assert.AreEqual(0, statements.Count);
        }

        [Test]
        public void SimpleStatement()
        {
            var statements = ParseInput(" Get-Location ")
                .EndBlock
                .Statements;

            Assert.AreEqual(1, statements.Count);
        }

        [Test]
        public void LastStatementGetsTerminatedInAddition()
        {
            var statements = ParseInput(" Get-Location;; ")
                .EndBlock
                .Statements;

            Assert.AreEqual(1, statements.Count);
        }

        [Test]
        public void StatementListCanStartWithTerminators()
        {
            var statements = ParseInput(" ;  ; Get-Location")
                .EndBlock
                .Statements;

            Assert.AreEqual(1, statements.Count);
        }

        [Test]
        public void StatementSequenceWithSemicolon()
        {
            var statements = ParseInput("Set-Location ; Get-Location")
                    .EndBlock
                    .Statements;

            Assert.AreEqual(2, statements.Count);
        }

        [Test]
        public void StatementSequenceWithoutSemicolonTest()
        {
            var statements = ParseInput("if ($true) { } Get-Location").EndBlock.Statements;
            Assert.AreEqual(2, statements.Count);
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
            ConstantExpressionAst ParseConstantExpression(string input)
            {
                return ParseExpression(input);
            }

            [Test]
            [TestCase("0", 0, typeof(System.Int32))]
            [TestCase("1", 1, typeof(System.Int32))]
            // int.MaxValue
            [TestCase("2147483647", 2147483647, typeof(System.Int32))]
            // int.MaxValue + 1
            [TestCase("2147483648", 2147483648, typeof(System.Int64))]
            // long.MaxValue
            [TestCase("9223372036854775807", 9223372036854775807, typeof(System.Int64))]
            // long.MaxValue + 1
            [TestCase("9223372036854775808", 9223372036854775808, typeof(System.Decimal))]
            // decimal values are not allowed as attribute arguments, so they are tested
            // separately (see IntegerLiteralDecimalExpression)
            // decimal.MaxValue + 1
            [TestCase("79228162514264337593543950336", 79228162514264337593543950336d, typeof(System.Double))]
            public void IntegerLiteralExpression(string expression, object value, Type type)
            {
                var ast = ParseConstantExpression(expression);
                Assert.AreEqual(value, ast.Value);
                Assert.AreEqual(type, ast.StaticType);
                Assert.AreEqual(expression, ast.Extent.Text);
            }

            [Test]
            [TestCase("-1", -1, typeof(System.Int32))]
            // int.MinValue + 1
            [TestCase("-2147483647", -2147483647, typeof(System.Int32))]
            // int.MinValue
            [TestCase("-2147483648", -2147483648, typeof(System.Int32))]
            // int.MinValue - 1
            [TestCase("-2147483649", -2147483649, typeof(System.Int64))]
            // int.MinValue as long literal should be long
            [TestCase("-2147483648l", -2147483648L, typeof(System.Int64), Explicit = true, Reason = "Returns an int")]
            // long.MinValue
            [TestCase("-9223372036854775807", -9223372036854775807, typeof(System.Int64))]
            // long.MinValue - 1
            [TestCase("-9223372036854775808", -9223372036854775808, typeof(System.Int64))]
            // decimal.MinValue - 1
            [TestCase("-79228162514264337593543950336", -79228162514264337593543950336d, typeof(System.Double))]
            public void NegativeIntegerLiteralExpression(string literal, object value, Type type)
            {
                var ast = ParseConstantExpression(literal);
                Assert.AreEqual(value, ast.Value);
                Assert.AreEqual(type, ast.StaticType);
            }

            [Test]
            [TestCase("0d", "0")]
            [TestCase("1d", "1")]
            [TestCase("-1d", "-1")]
            // decimal.MaxValue
            [TestCase("79228162514264337593543950335", "79228162514264337593543950335")]
            // decimal.MinValue
            [TestCase("-79228162514264337593543950335", "-79228162514264337593543950335")]
            [TestCase("-9223372036854775808d", "-9223372036854775808", Explicit = true, Reason = "Returns a long")]
            public void IntegerLiteralDecimalExpression(string expression, string value)
            {
                var ast = ParseConstantExpression(expression);
                Assert.AreEqual(Decimal.Parse(value), ast.Value);
                Assert.AreEqual(typeof(System.Decimal), ast.StaticType);
            }

            [Test]
            public void IntegerLiteralVeryLargeNumberShouldBeDouble()
            {
                var type = ParseConstantExpression(new string('9', 200)).StaticType;
                Assert.AreEqual(typeof(System.Double), type);
            }

            [Test, ExpectedException(typeof(OverflowException))]
            public void IntegerLiteralVeryVeryVeryLargeNumberShouldThrow()
            {
                ParseConstantExpression(new string('9', 310));
            }

            [Test, Combinatorial]
            public void LongIntegerLiteralShouldBeInt64(
                [Values("0", "2147483647", "2147483648", "9223372036854775807")]
                string literal,
                [Values("l", "L")]
                string typeSuffix)
            {
                var type = ParseConstantExpression(literal + typeSuffix).StaticType;
                Assert.AreEqual(type, typeof(System.Int64));
            }

            [Test, ExpectedException(typeof(ArithmeticException))]
            [TestCase("l")]
            [TestCase("L")]
            public void LongIntegerLiteralLargeNumberShouldThrow(string typeSuffix)
            {
                ParseConstantExpression("9223372036854775808" + typeSuffix);
            }

            [Test]
            [TestCase("1kb", 1024, typeof(System.Int32))]
            [TestCase("1mb", 1048576, typeof(System.Int32))]
            [TestCase("1MB", 1048576, typeof(System.Int32))]
            [TestCase("1gb", 1073741824, typeof(System.Int32))]
            [TestCase("100gb", 107374182400, typeof(System.Int64))]
            [TestCase("1tb", 1099511627776, typeof(System.Int64))]
            [TestCase("1pb", 1125899906842624, typeof(System.Int64))]
            [TestCase("9876543210kb", 10113580247040, typeof(System.Int64))]
            [TestCase("12lkb", 12288, typeof(System.Int64))]
            // long with overflow into double
            [TestCase("9223372036854775807pb", 1.0384593717069655e34, typeof(System.Double))]
            // decimal with overflow into double
            [TestCase("79228162514264337593543950335kb", 8.1129638414606682e31, typeof(System.Double))]
            [TestCase("79228162514264337593543950335dkb", 8.1129638414606682e31, typeof(System.Double))]
            // double with multiplier
            [TestCase("1000000000000000000000000000000kb", 1.024e33, typeof(System.Double))]
            [TestCase("1000000000000000000000000000000pb", 1.125899906842624e45, typeof(System.Double))]
            public void IntegerWithNumericMultiplier(string expression, object result, Type type)
            {
                var ast = ParseConstantExpression(expression);
                Assert.AreEqual(result, ast.Value);
                Assert.AreEqual(type, ast.StaticType);
            }

            // int with overflow into decimal
            [TestCase("2147483647pb", "2417851638103358442569728")]
            // long with overflow into decimal
            [TestCase("9223372036854775807kb", "9444732965739290426368")]
            // decimal without overflow
            [TestCase("10000000000000000000kb", "10240000000000000000000")]
            // decimal literal without overflow
            [TestCase("1dpb", "1125899906842624")]
            public void IntegerWithNumericMultiplierDecimalResult(string expression, string result)
            {
                var ast = ParseConstantExpression(expression);
                Assert.AreEqual(Decimal.Parse(result), ast.Value);
                Assert.AreEqual(typeof(System.Decimal), ast.StaticType);
            }

            [Test, Combinatorial]
            public void DecimalIntegerLiteralShouldBeDecimal(
                [Values("0", "2147483647", "2147483648", "9223372036854775807", "9223372036854775808")]
                string literal,
                [Values("d", "D")]
                string typeSuffix)
            {
                var type = ParseConstantExpression(literal + typeSuffix).StaticType;
                Assert.AreEqual(type, typeof(System.Decimal));
            }

            [Test, ExpectedException(typeof(ArithmeticException))]
            [TestCase("d")]
            [TestCase("D")]
            public void DecimalIntegerLiteralLargeNumberShouldThrow(string typeSuffix)
            {
                ParseExpression("79228162514264337593543950336" + typeSuffix);
            }

            [Test]
            [TestCase("0x0", 0x0, typeof(System.Int32))]
            [TestCase("0xa", 0xa, typeof(System.Int32))]
            [TestCase("0x7fffffff", 0x7fffffff, typeof(System.Int32))]
            [TestCase("0x100000000", 0x100000000L, typeof(System.Int64))]
            [TestCase("0x7fffffffffffffff", 0x7fffffffffffffffL, typeof(System.Int64))]
            [TestCase("0x80000000", -2147483648, typeof(System.Int32))]
            [TestCase("0xffffffff", -1, typeof(System.Int32))]
            [TestCase("0x8000000000000000", -9223372036854775808, typeof(System.Int64))]
            [TestCase("0xffffffffffffffff", -1, typeof(System.Int64))]
            public void HexIntegerLiteralExpression(string literal, object result, Type type)
            {
                var ast = ParseConstantExpression(literal);
                Assert.AreEqual(result, ast.Value);
                Assert.AreEqual(type, ast.StaticType);
                Assert.AreEqual(literal, ast.Extent.Text);
            }

            [Test, ExpectedException(typeof(OverflowException))]
            public void HexIntegerLiteralTooBigForLong()
            {
                ParseConstantExpression("0x10000000000000000");
            }

            [Test]
            [TestCase("0x12kb", 18432)]
            [TestCase("0x12mb", 18874368)]
            [TestCase("0x12gb", 19327352832)]
            [TestCase("0x12tb", 19791209299968)]
            [TestCase("0x12pb", 20266198323167232)]
            [TestCase("0x800000000000000pb", 649037107316853453566312041152512.0)]
            public void HexIntegerWithNumericMultiplierTest(string expression, object expectedResult)
            {
                var result = ParseConstantExpression(expression).Value;
                Assert.AreEqual(expectedResult, result);
            }
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
            var exp = ParseStatement(@"'x' + `" + Environment.NewLine + "'y'").PipelineElements[0].Expression;
            Assert.NotNull(exp);
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
            Parser.IronyParser.Context.TracingEnabled = true;
            return Parser.ParseInput(s);
        }

        static dynamic ParseStatement(string input)
        {
            return ParseInput(input)
                .EndBlock
                .Statements[0];
        }

        static dynamic ParseExpression(string input)
        {
            return ParseStatement(input)
                .PipelineElements[0]
                .Expression;
        }

        [TestFixture]
        public class MemberAccess
        {
            [Test]
            public void StaticProperty()
            {
                var exp = ParseStatement("[System.Int32]::MaxValue").PipelineElements[0].Expression;
                Assert.NotNull(exp);
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
                Assert.Throws<ParseException>(() =>
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
        [TestCase("0x0F0F -band 0xFE", TokenKind.Band)]
        [TestCase("0x0F0F -bor 0xFE", TokenKind.Bor)]
        [TestCase("0x0F0F -bxor 0xFE", TokenKind.Bxor)]
        public void BitwiseOperator(string input, TokenKind expectedOperator)
        {
            BinaryExpressionAst binaryExpressionAst = ParseStatement(input)
                .PipelineElements[0]
                .Expression;

            Assert.AreEqual(expectedOperator, binaryExpressionAst.Operator);
        }

        [Test]
        public void BitwiseOperatorInisdeMemberInvoke()
        {
            string input = "$obj.GetType().GetMethods(0x0F -bor 0xFE)";
            BinaryExpressionAst binaryExpressionAst = ParseStatement(input)
                .PipelineElements[0]
                .Expression
                .Arguments[0];

            Assert.AreEqual(TokenKind.Bor, binaryExpressionAst.Operator);
            Assert.AreEqual(254, ((ConstantExpressionAst)binaryExpressionAst.Right).Value);
            Assert.AreEqual(15, ((ConstantExpressionAst)binaryExpressionAst.Left).Value);
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
        public void CastWithArray()
        {
            ConvertExpressionAst convertExpressionAst = ParseStatement("[Byte[]] 5")
                .PipelineElements[0]
                .Expression;

            Assert.AreEqual(true, convertExpressionAst.Type.TypeName.IsArray);
            Assert.AreEqual("Byte", convertExpressionAst.Type.TypeName.Name);
            Assert.AreEqual(5, ((ConstantExpressionAst)convertExpressionAst.Child).Value);
        }

        [Test]
        public void For3Arguments()
        {
            Assert.DoesNotThrow(delegate {
                ParseStatement("for ($i = 0; $i -ile 10; $i++) {Write-Host $i}");
            });
        }

        [Test]
        public void For2Arguments()
        {
            ForStatementAst forStatementAst = ParseStatement("for ($i = 0; $i -ile 10) {Write-Host $i}");
        }

        [Test]
        public void For1Argument()
        {
            ForStatementAst forStatementAst = ParseStatement("for ($i = 0) {Write-Host $i}");
        }

        [Test]
        public void ForEmpty()
        {
            ForStatementAst forStatementAst = ParseStatement("for () {Write-Host $i}");
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

        [Test]
        public void Throw()
        {
            ThrowStatementAst throwStatementAst = ParseStatement("{ throw }")
                .PipelineElements[0]
                .Expression
                .ScriptBlock
                .EndBlock
                .Statements[0];

            Assert.IsNull(throwStatementAst.Pipeline);
        }

        [Test]
        public void ThrowString()
        {
            ThrowStatementAst throwStatementAst = ParseStatement("{ throw 'error' }")
                .PipelineElements[0]
                .Expression
                .ScriptBlock
                .EndBlock
                .Statements[0];

            Assert.IsNotNull(throwStatementAst.Pipeline);
        }

        [Test]
        public void Trap()
        {
            NamedBlockAst namedBlockAst = ParseInput("trap {}")
                .EndBlock;

            var trapStatementAst = namedBlockAst.Traps.First();
            Assert.IsNull(trapStatementAst.TrapType);
            Assert.AreEqual(0, trapStatementAst.Body.Statements.Count);
            Assert.AreEqual(1, namedBlockAst.Traps.Count);
            Assert.AreEqual(0, namedBlockAst.Statements.Count);
        }

        [Test]
        public void TrapWithTypeConstraint()
        {
            NamedBlockAst namedBlockAst = ParseInput("trap [System.FormatException] {}")
                .EndBlock;

            var trapStatementAst = namedBlockAst.Traps.First();
            Assert.AreEqual("System.FormatException", trapStatementAst.TrapType.TypeName.Name);
        }

        [Test]
        public void Continue()
        {
            ContinueStatementAst continueStatementAst = ParseStatement("continue");

            Assert.IsNull(continueStatementAst.Label);
        }

        [Test]
        public void Break()
        {
            BreakStatementAst breakStatementAst = ParseStatement("break");

            Assert.IsNull(breakStatementAst.Label);
        }

        [TestFixture]
        public class ExpandableStringExpressionTests
        {
            dynamic Parse(string input)
            {
                return ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;
            }

            [Test]
            public void VariableInsideDoubleQuotedString()
            {
                ExpandableStringExpressionAst expandableStringAst = Parse("\"$foo\"");

                var variableAst = expandableStringAst.NestedExpressions.FirstOrDefault() as VariableExpressionAst;
                Assert.AreEqual(StringConstantType.DoubleQuoted, expandableStringAst.StringConstantType);
                Assert.AreEqual(typeof(string), expandableStringAst.StaticType);
                Assert.AreEqual("$foo", expandableStringAst.Value);
                Assert.AreEqual("foo", variableAst.VariablePath.UserPath);
            }

            [Test]
            public void ExpandableStringVariableInsideDoubleQuotedStringHasExtentThatIncludesDoubleQuotes()
            {
                ExpandableStringExpressionAst expandableStringAst = Parse("\"$a\"");

                IScriptExtent extent = expandableStringAst.Extent;
                var expectedExtent = new ExpectedScriptExtent
                {
                    Text = "\"$a\"",
                    StartOffset = 0,
                    EndOffset = 4,
                    StartColumnNumber = 1,
                    EndColumnNumber = 5,
                    EndLineNumber = 1,
                    StartLineNumber = 1
                };
                expectedExtent.AssertAreEqual(extent);
            }

            [Test]
            [TestCase("\"$abc\"", "$abc", 1, 5, 2, 6, 1, 1)]
            [TestCase("\"-$abc-\"", "$abc", 2, 6, 3, 7, 1, 1)]
            [TestCase("\"`$a=$a.\"", "$a", 5, 7, 6, 8, 1, 1)]
            [TestCase("\"  $abc\"", "$abc", 3, 7, 4, 8, 1, 1)]
            [TestCase("\r\n\"$abc\"", "$abc", 3, 7, 2, 6, 2, 2)]
            public void VariableInsideDoubleQuotedStringHasScriptExtentDefined(
                string input,
                string extentText,
                int startOffset,
                int endOffset,
                int startColumn,
                int endColumn,
                int startLine,
                int endLine)
            {
                ExpandableStringExpressionAst expandableStringAst = Parse(input);

                var variableAst = expandableStringAst.NestedExpressions.FirstOrDefault() as VariableExpressionAst;
                IScriptExtent extent = variableAst.Extent;

                var expectedExtent = new ExpectedScriptExtent
                {
                    Text = extentText,
                    StartOffset = startOffset,
                    EndOffset = endOffset,
                    StartColumnNumber = startColumn,
                    EndColumnNumber = endColumn,
                    StartLineNumber = startLine,
                    EndLineNumber = endLine
                };
                expectedExtent.AssertAreEqual(extent);
            }

            /// <summary>
            /// PowerShell only creates an ExpandableStringExpressionAst if the string needs to be
            /// expanded.
            /// </summary>
            [Test]
            [TestCase("\"abc\"")]
            [TestCase("\"$\"")]
            public void DoubleQuotedStringContainingConstantStringShouldBeTreatedAsStringConstantNotExpandableString(string input)
            {
                StringConstantExpressionAst stringConstantAst = Parse(input);

                Assert.AreEqual(StringConstantType.DoubleQuoted, stringConstantAst.StringConstantType);
            }

            [Test]
            public void VariableFollowedByDotCharacterShouldNotIncludeDotInVariableName()
            {
                ExpandableStringExpressionAst expandableStringAst = Parse("\"$foo.\"");

                var variableAst = expandableStringAst.NestedExpressions.FirstOrDefault() as VariableExpressionAst;
                Assert.AreEqual("foo", variableAst.VariablePath.UserPath);
            }

            [Test]
            public void TwoVariablesNextToEachOtherBothAreFound()
            {
                ExpandableStringExpressionAst expandableStringAst = Parse("\"$foo$bar\"");

                var firstVariableAst = expandableStringAst.NestedExpressions.FirstOrDefault() as VariableExpressionAst;
                var lastVariableAst = expandableStringAst.NestedExpressions.LastOrDefault() as VariableExpressionAst;
                Assert.AreEqual("foo", firstVariableAst.VariablePath.UserPath);
                Assert.AreEqual("bar", lastVariableAst.VariablePath.UserPath);
                Assert.AreEqual(2, expandableStringAst.NestedExpressions.Count);
            }

            [Test]
            public void EscapedDollarSignFollowedByLettersShouldNotBeTreatedAsVariable()
            {
                ExpandableStringExpressionAst expandableStringAst = Parse("\"`$foo $bar\"");

                var variableAst = expandableStringAst.NestedExpressions.FirstOrDefault() as VariableExpressionAst;
                Assert.AreEqual("bar", variableAst.VariablePath.UserPath);
                Assert.AreEqual(1, expandableStringAst.NestedExpressions.Count);
            }

            [Test]
            public void SingleCharacterVariableName()
            {
                ExpandableStringExpressionAst expandableStringAst = Parse("\"$a\"");

                var variableAst = expandableStringAst.NestedExpressions.FirstOrDefault() as VariableExpressionAst;
                Assert.AreEqual("a", variableAst.VariablePath.UserPath);
            }
        }

        [TestFixture]
        public class ScriptExtentTests
        {
            [Test]
            [TestCase("$abc")]
            [TestCase("")]
            [TestCase("\"$a\"")]
            public void SingleLineScriptBlock(string input)
            {
                IScriptExtent extent = ParseInput(input)
                    .Extent;

                Assert.AreEqual(input, extent.Text);
                Assert.AreEqual(0, extent.StartOffset);
                Assert.AreEqual(input.Length, extent.EndOffset);
                Assert.AreEqual(1, extent.StartLineNumber);
                Assert.AreEqual(1, extent.EndLineNumber);
                Assert.AreEqual(1, extent.StartColumnNumber);
                Assert.AreEqual(input.Length + 1, extent.EndColumnNumber);
                Assert.AreEqual(null, extent.File);
            }
        }

        [TestFixture]
        public abstract class RealLiteralExpressionTests
        {
            dynamic ParseConstantExpression(string input)
            {
                return ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;
            }

            [Test]
            public void Value()
            {
                ConstantExpressionAst expression = ParseConstantExpression("1.2");
                Assert.AreEqual(1.2, expression.Value);
            }

            [Test]
            public void StaticType()
            {
                ConstantExpressionAst expression = ParseConstantExpression("1.2");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void Text()
            {
                ConstantExpressionAst expression = ParseConstantExpression("1.2");
                Assert.AreEqual("1.2", expression.Extent.Text);
            }

            [Test]
            public void StaticTypeExponentWithNoSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("3.45e3");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            [Ignore("Grammar does not support this. PowerShell spec does not include this even though it is supported")]
            public void StaticTypeDotPrecedesSingleDigitExponentWithNoSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("3.e3");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void StaticTypeStartingDotThenFollowedByExponentWithNoSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression(".45e35");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void StaticTypeExponentNoSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("2.45e35");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void StaticTypeSingleDigitExponentWithPositiveSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("32.2e+3");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            [Ignore("Grammar does not support this. PowerShell spec does not include this even though it is supported")]
            public void StaticTypeDotPrecedingExponentWithPositiveSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("32.e+12");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void StaticTypeSingleDigitExponentWithNegativeSignSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("123.456e-2");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void StaticTypeExponentWithNegativeSignSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("123.456e-231");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void StaticTypeExponentInUpperCaseWithNegativeSignSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("123.456E-231");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            [Ignore("Grammar does not support this. PowerShell spec does not include this even though it is supported")]
            public void StaticTypeDotPrecedingExponentWithNegativeSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("32.e+12");
                Assert.AreEqual(typeof(double), expression.StaticType);
            }

            [Test]
            public void TooBigForDouble()
            {
                Assert.Throws<OverflowException>(() => ParseConstantExpression("2.2e500"));
            }

            [Test]
            public void IncorrectExponentPartPattern()
            {
                // The grammar incorrectly accepted 1|2 as a real literal, but
                // would generate an OverflowException for it. Thus, we test
                // that the grammar does not even attempt to parse it as a real
                // literal and gives us the expected PipelineAst instead.
                ScriptBlockAst ast = ParseInput("1|2");
                Assert.IsInstanceOf(typeof(PipelineAst), ast.EndBlock.Statements[0]);
            }

            [Test]
            [TestCase("1.5mb", 1572864)]
            [TestCase("1.5MB", 1572864)]
            [TestCase("1.5kb", 1536)]
            [TestCase("1.5gb", 1610612736)]
            [TestCase("0.5tb", 549755813888)]
            [TestCase("0.5pb", 562949953421312)]
            public void NumericMultiplier(string expression, double result)
            {
                ConstantExpressionAst constExpression = ParseConstantExpression(expression);
                Assert.AreEqual(typeof(double), constExpression.StaticType);
                Assert.AreEqual(result, constExpression.Value);
            }
        }

        [TestFixture]
        [SetCulture("en-US")]
        public class USLocaleRealLiteralExpressionTests : RealLiteralExpressionTests
        {
        }

        [TestFixture]
        [SetCulture("de-DE")]
        public class GermanLocaleRealLiteralExpressionTests : RealLiteralExpressionTests
        {
        }

        [TestFixture]
        public abstract class DecimalRealLiteralExpressionTests
        {
            dynamic ParseConstantExpression(string input)
            {
                return ParseStatement(input)
                    .PipelineElements[0]
                    .Expression;
            }

            [Test]
            public void Value()
            {
                ConstantExpressionAst expression = ParseConstantExpression("1.2d");
                Assert.AreEqual(1.2m, expression.Value);
            }

            [Test]
            public void StaticType()
            {
                ConstantExpressionAst expression = ParseConstantExpression("1.2d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            public void Text()
            {
                ConstantExpressionAst expression = ParseConstantExpression("1.2d");
                Assert.AreEqual("1.2d", expression.Extent.Text);
            }

            [Test]
            public void StaticTypeExponentWithNoSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("3.45e3d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            [Ignore("Grammar does not support this. PowerShell spec does not include this even though it is supported")]
            public void StaticTypeDotPrecedesSingleDigitExponentWithNoSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("3.e3d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            public void StaticTypeSingleDigitExponentWithPositiveSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("32.2e+3d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            [Ignore("Treats '32.' as integer.")]
            public void StaticTypeDotPrecedingExponentWithPositiveSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("32.e+12d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            public void StaticTypeSingleDigitExponentWithNegativeSignSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("123.456e-2d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            public void StaticTypeExponentWithNegativeSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("123.456e-231D");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            public void StaticTypeExponentInUpperCaseWithNegativeSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("123.456E-231d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            [Ignore("Grammar does not support this. PowerShell spec does not include this even though it is supported")]
            public void StaticTypeDotPrecedingExponentWithNegativeSign()
            {
                ConstantExpressionAst expression = ParseConstantExpression("32.e+12d");
                Assert.AreEqual(typeof(decimal), expression.StaticType);
            }

            [Test]
            [TestCase("1.5Dmb", 1572864)]
            [TestCase("1.5dMB", 1572864)]
            [TestCase("1.5Dkb", 1536)]
            [TestCase("1.5dgb", 1610612736)]
            [TestCase("0.5Dtb", 549755813888.0)]
            [TestCase("0.5Dpb", 562949953421312.0)]
            public void NumericMultiplier(string expression, decimal result)
            {
                ConstantExpressionAst constExpression = ParseConstantExpression(expression);
                Assert.AreEqual(typeof(decimal), constExpression.StaticType);
                Assert.AreEqual(result, constExpression.Value);
            }
        }

        [TestFixture]
        [SetCulture("en-US")]
        public class USLocaleDecimalRealLiteralExpressionTests : DecimalRealLiteralExpressionTests
        {
        }

        [TestFixture]
        [SetCulture("de-DE")]
        public class GermanLocaleDecimalRealLiteralExpressionTests : DecimalRealLiteralExpressionTests
        {
        }

        [TestFixture]
        public class SubExpressionTests
        {
            [Test]
            public void IntegerAdditionSubExpression()
            {
                SubExpressionAst subExpressionAst = ParseExpression("$(1+2)");
                var pipelineAst = subExpressionAst.SubExpression.Statements[0] as PipelineAst;
                var commandExpressionAst = pipelineAst.PipelineElements[0] as CommandExpressionAst;
                var binaryExpressionAst = commandExpressionAst.Expression as BinaryExpressionAst;
                var leftExpressionAst = binaryExpressionAst.Left as ConstantExpressionAst;
                var rightExpressionAst = binaryExpressionAst.Right as ConstantExpressionAst;

                Assert.AreEqual(TokenKind.Plus, binaryExpressionAst.Operator);
                Assert.AreEqual(leftExpressionAst.Value, 1);
                Assert.AreEqual(rightExpressionAst.Value, 2);
            }
        }

        [TestFixture]
        public class FileRedirectionTests
        {
            dynamic Parse(string input)
            {
                return ParseStatement(input)
                    .PipelineElements[0];
            }

            [Test]
            public void OutputStreamToFile()
            {
                CommandAst commandAst = Parse("Get-Process > out.txt");
                var firstCommandElementAst = commandAst.CommandElements.FirstOrDefault() as StringConstantExpressionAst;
                var redirectAst = commandAst.Redirections.FirstOrDefault() as FileRedirectionAst;
                var redirectFileNameAst = redirectAst.Location as StringConstantExpressionAst;

                Assert.AreEqual(1, commandAst.CommandElements.Count);
                Assert.IsNotNull(firstCommandElementAst);
                Assert.AreEqual("Get-Process", firstCommandElementAst.Value);
                Assert.AreEqual(1, commandAst.Redirections.Count);
                Assert.IsFalse(redirectAst.Append);
                Assert.AreEqual(RedirectionStream.Output, redirectAst.FromStream);
                Assert.IsNotNull(redirectFileNameAst);
                Assert.AreEqual("out.txt", redirectFileNameAst.Value);
            }

            [Test]
            public void OutputStreamAppendedToFile()
            {
                CommandAst commandAst = Parse("Get-Process >> out.txt");
                var firstCommandElementAst = commandAst.CommandElements.FirstOrDefault() as StringConstantExpressionAst;
                var redirectAst = commandAst.Redirections.FirstOrDefault() as FileRedirectionAst;
                var redirectFileNameAst = redirectAst.Location as StringConstantExpressionAst;

                Assert.IsTrue(redirectAst.Append);
                Assert.AreEqual(RedirectionStream.Output, redirectAst.FromStream);
                Assert.AreEqual("out.txt", redirectFileNameAst.Value);
            }

            [Test]
            public void CommandExpressionOutputToFile()
            {
                CommandExpressionAst commandExpressionAst = Parse("$i > out.txt");
                var variableExpressionAst = commandExpressionAst.Expression as VariableExpressionAst;
                var redirectAst = commandExpressionAst.Redirections.FirstOrDefault() as FileRedirectionAst;
                var redirectFileNameAst = redirectAst.Location as StringConstantExpressionAst;

                Assert.AreEqual("i", variableExpressionAst.VariablePath.ToString());
                Assert.AreEqual(1, commandExpressionAst.Redirections.Count);
                Assert.IsFalse(redirectAst.Append);
                Assert.AreEqual(RedirectionStream.Output, redirectAst.FromStream);
                Assert.AreEqual("out.txt", redirectFileNameAst.Value);
            }
        }
    }
}

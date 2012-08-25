using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using Extensions.String;
using Pash.ParserIntrinsics;

namespace ParserTests
{
    [TestFixture]
    class PowerShellGrammarTests
    {
        [Test]
        public void CreateTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();
            // obviously I know it won't be null. That's mostly to 
            // avoid the compiler warning.
            Assert.IsNotNull(grammar);
        }

        [Test]
        public void CaseInsensitiveTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();
            Assert.True(!grammar.CaseSensitive);
        }

        [Test]
        public void NonTerminalFieldsInitializedTest()
        {
            // created by reflection (to avoid missing one)
            // but let's make sure the reflection works
            var grammar = new PowerShellGrammar.InteractiveInput();
            Assert.IsNotNull(grammar.interactive_input);
            Assert.AreEqual("interactive_input", grammar.interactive_input.Name);
        }

        // Separate tests for each level of reported grammar error, so it's easy to diagnose our current
        // status, and to mark [Ignore] to our level of tolerance.
        [TestFixture]
        public class GrammarErrorLevelTests
        {
            [Test]
            public void LanguageErrorLevelPerfect()
            {
                var grammar = new PowerShellGrammar.InteractiveInput();
                var languageData = new LanguageData(grammar);
                Assert.AreEqual(languageData.ErrorLevel, GrammarErrorLevel.NoError, languageData.Errors.JoinString("\n"));
            }

            [Test]
            public void LanguageErrorLevelNoInfos()
            {
                var grammar = new PowerShellGrammar.InteractiveInput();
                var languageData = new LanguageData(grammar);
                Assert.Less(languageData.ErrorLevel, GrammarErrorLevel.Info, languageData.Errors.JoinString("\n"));
            }

            [Test]
            public void LanguageErrorLevelNoWarnings()
            {
                var grammar = new PowerShellGrammar.InteractiveInput();
                var languageData = new LanguageData(grammar);
                Assert.Less(languageData.ErrorLevel, GrammarErrorLevel.Warning, languageData.Errors.JoinString("\n"));
            }

            [Test]
            public void LanguageErrorLevelNoConflicts()
            {
                var grammar = new PowerShellGrammar.InteractiveInput();
                var languageData = new LanguageData(grammar);
                Assert.Less(languageData.ErrorLevel, GrammarErrorLevel.Conflict, languageData.Errors.JoinString("\n"));
            }


            [Test]
            public void LanguageErrorLevelNoErrors()
            {
                var grammar = new PowerShellGrammar.InteractiveInput();
                var languageData = new LanguageData(grammar);
                Assert.Less(languageData.ErrorLevel, GrammarErrorLevel.Error, languageData.Errors.JoinString("\n"));
            }


            [Test]
            public void LanguageErrorLevelNoInternalErrors()
            {
                var grammar = new PowerShellGrammar.InteractiveInput();
                var languageData = new LanguageData(grammar);
                Assert.Less(languageData.ErrorLevel, GrammarErrorLevel.InternalError, languageData.Errors.JoinString("\n"));
            }
        }

        [TestFixture]
        class ParseSimpleCommandTest
        {
            ParseTree parseTree;
            PowerShellGrammar grammar;

            public ParseSimpleCommandTest()
            {
                grammar = new PowerShellGrammar.InteractiveInput();
                var parser = new Parser(grammar);

                parseTree = parser.Parse("Get-ChildItem");
            }

            [Test]
            public void SuccessfulParseTest()
            {
                Assert.IsNotNull(parseTree);
                Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));
            }

            [Test]
            public void CorrectNonTerminalsTest()
            {
                var node = VerifyParseTreeSingles(parseTree.Root,
                    grammar.interactive_input,
                    grammar.script_block,
                    grammar.script_block_body,
                    grammar.statement_list,
                    grammar.statement,
                    grammar.pipeline,
                    grammar.command,
                    grammar.command_name
                );

                Assert.AreEqual(0, node.ChildNodes.Count, node.ToString());
            }
        }

        [Test]
        public void TrivialPromptExpressionsTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("\"PS> \"");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var node = VerifyParseTreeSingles(parseTree.Root,
                grammar.interactive_input,
                grammar.script_block,
                grammar.script_block_body,
                grammar.statement_list,
                grammar.statement,
                grammar.pipeline,
                grammar.expression,
                grammar.logical_expression,
                grammar.bitwise_expression,
                grammar.comparison_expression,
                grammar.additive_expression,
                grammar.multiplicative_expression,
                grammar.format_expression,
                grammar.array_literal_expression,
                grammar.unary_expression,
                grammar.primary_expression,
                grammar.value,
                grammar.literal,
                grammar.string_literal
            );

            Assert.AreEqual(0, node.ChildNodes.Count, node.ToString());
            Assert.AreEqual(PowerShellGrammar.Terminals.expandable_string_literal, node.Term);
        }

        static ParseTreeNode VerifyParseTreeSingles(ParseTreeNode node, params NonTerminal[] expected)
        {
            foreach (var rule in expected)
            {
                Assert.AreEqual(rule, node.Term);
                Assert.AreEqual(1, node.ChildNodes.Count, "wrong child count.\n" + FormatNodes(node));
                node = node.ChildNodes.Single();
            }

            return node;
        }

        static string FormatNodes(ParseTreeNode node, int indent = 1)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(new string('\t', indent));
            stringBuilder.AppendLine(node.ToString());

            foreach (var childNode in node.ChildNodes)
            {
                stringBuilder.Append(FormatNodes(childNode, indent + 1));
            }

            return stringBuilder.ToString();
        }

        // the default prompt expression from app.config, slightly simplified just to
        // make the test easier to write.
        [Test]
        public void DefaultPromptExpressionsTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("'PS> ' + (Get-Location)");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var node = VerifyParseTreeSingles(parseTree.Root,
                grammar.interactive_input,
                grammar.script_block,
                grammar.script_block_body,
                grammar.statement_list,
                grammar.statement,
                grammar.pipeline,
                grammar.expression,
                grammar.logical_expression,
                grammar.bitwise_expression,
                grammar.comparison_expression
            );

            Assert.AreEqual(grammar.additive_expression, node.Term);
            Assert.AreEqual(3, node.ChildNodes.Count, node.ToString());

            var leftNode = node.ChildNodes[0];
            var operatorNode = node.ChildNodes[1];
            var rightNode = node.ChildNodes[2];

            {
                var leftLiteral = VerifyParseTreeSingles(leftNode,
                    grammar.additive_expression,
                    grammar.multiplicative_expression,
                    grammar.format_expression,
                    grammar.array_literal_expression,
                    grammar.unary_expression,
                    grammar.primary_expression,
                    grammar.value,
                    grammar.literal,
                    grammar.string_literal
                );

                Assert.AreEqual(PowerShellGrammar.Terminals.verbatim_string_literal, leftLiteral.Term);
            }

            {
                KeywordTerminal keywordTerminal = (KeywordTerminal)operatorNode.Term;
                Assert.AreEqual("+", keywordTerminal.Text);
            }

            {
                var nodeX = VerifyParseTreeSingles(rightNode,
                    grammar.multiplicative_expression,
                    grammar.format_expression,
                    grammar.array_literal_expression,
                    grammar.unary_expression,
                    grammar.primary_expression,
                    grammar.value
                );

                Assert.AreEqual(grammar.parenthesized_expression, nodeX.Term);
                Assert.AreEqual(3, nodeX.ChildNodes.Count);

                KeywordTerminal leftParenTerminal = (KeywordTerminal)nodeX.ChildNodes[0].Term;
                Assert.AreEqual("(", leftParenTerminal.Text);

                KeywordTerminal rightParenTerminal = (KeywordTerminal)nodeX.ChildNodes[2].Term;
                Assert.AreEqual(")", rightParenTerminal.Text);

                var pipelineNode = nodeX.ChildNodes[1];

                var command_name_token = VerifyParseTreeSingles(pipelineNode,
                    grammar.pipeline,
                    grammar.command,
                    grammar.command_name
                );

                Assert.AreEqual(PowerShellGrammar.Terminals.generic_token, command_name_token.Term);
                Assert.AreEqual("Get-Location", command_name_token.FindTokenAndGetText());
            }
        }

        [Test]
        public void NumberOverCommandTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("1");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var node = VerifyParseTreeSingles(parseTree.Root,
                grammar.interactive_input,
                grammar.script_block,
                grammar.script_block_body,
                grammar.statement_list,
                grammar.statement,
                grammar.pipeline,
                grammar.expression,
                grammar.logical_expression,
                grammar.bitwise_expression,
                grammar.comparison_expression,
                grammar.additive_expression,
                grammar.multiplicative_expression,
                grammar.format_expression,
                grammar.array_literal_expression,
                grammar.unary_expression,
                grammar.primary_expression,
                grammar.value,
                grammar.literal,
                grammar.integer_literal
            );

            Assert.AreEqual(PowerShellGrammar.Terminals.decimal_integer_literal, node.Term);
        }

        [Test]
        public void ParametersTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse(@"Set-Location C:\Windows");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var commandNode = VerifyParseTreeSingles(parseTree.Root,
                grammar.interactive_input,
                grammar.script_block,
                grammar.script_block_body,
                grammar.statement_list,
                grammar.statement,
                grammar.pipeline
            );

            Assert.AreEqual(2, commandNode.ChildNodes.Count, commandNode.ToString());
            Assert.AreEqual(grammar.command_name, commandNode.ChildNodes[0].Term);

            var parametersNode = commandNode.ChildNodes[1];

            var node = VerifyParseTreeSingles(parametersNode,
                grammar.command_elements,
                grammar.command_element,
                grammar.command_argument,
                grammar.command_name_expr,
                grammar.command_name
                );

            Assert.AreEqual(PowerShellGrammar.Terminals.generic_token, node.Term);
        }
    }
}

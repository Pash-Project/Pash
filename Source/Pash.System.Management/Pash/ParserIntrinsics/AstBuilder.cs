using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Language;

using Extensions.String;
using Irony.Parsing;
using Pash.ParserIntrinsics;
using System.Text.RegularExpressions;

namespace Pash.ParserIntrinsics
{
    class AstBuilder
    {
        readonly PowerShellGrammar _grammar;

        public AstBuilder(PowerShellGrammar grammar)
        {
            this._grammar = grammar;
        }

        [Conditional("DEBUG")]
        static void VerifyTerm(ParseTreeNode parseTreeNode, BnfTerm expectedTerm, params BnfTerm[] moreExpectedTerms)
        {
            var allExpected = new[] { expectedTerm }.Concat(moreExpectedTerms);

            if (!allExpected.Where(node => parseTreeNode.Term == expectedTerm).Any())
            {
                throw new InvalidOperationException("expected '{0}' to be a '{1}'".FormatString(parseTreeNode, allExpected.ToArray().JoinString(", ")));
            }
        }

        public ScriptBlockAst BuildInteractiveInputAst(ParseTreeNode interactiveInputNode)
        {
            VerifyTerm(interactiveInputNode, this._grammar.interactive_input);

            ////        interactive_input:
            ////            script_block
            var scriptBlockNode = interactiveInputNode.ChildNodes.Single();
            VerifyTerm(scriptBlockNode, this._grammar.script_block);

            return BuildScriptBlockAst(scriptBlockNode);
        }

        ScriptBlockAst BuildScriptBlockAst(ParseTreeNode scriptBlockNode)
        {
            VerifyTerm(scriptBlockNode, this._grammar.script_block);
            ////        script_block:
            ////            param_block_opt   statement_terminators_opt    script_block_body_opt

            var scriptBlockBodyNode = scriptBlockNode.ChildNodes[0];
            VerifyTerm(scriptBlockBodyNode, this._grammar.script_block_body, this._grammar.Empty);
            return new ScriptBlockAst(
                new ScriptExtent(scriptBlockNode),
                null,
                scriptBlockBodyNode.Term == this._grammar.script_block_body ? BuildStatementBlockAst(scriptBlockBodyNode) : null,
                false
                );
        }

        StatementBlockAst BuildStatementBlockAst(ParseTreeNode scriptBlockBodyNode)
        {
            VerifyTerm(scriptBlockBodyNode, this._grammar.script_block_body/* TODO: , named_block_list*/);

            ////        script_block_body:
            ////            named_block_list
            ////            statement_list

            var statementListNode = scriptBlockBodyNode.ChildNodes.Single();

            return new StatementBlockAst(new ScriptExtent(scriptBlockBodyNode), GetStatements(statementListNode), new TrapStatementAst[] { });
        }

        IEnumerable<StatementAst> GetStatements(ParseTreeNode statementListNode)
        {
            ////        statement_list:
            ////            statement
            ////            statement_list   statement

            // HACK: I used
            //        statement_list:
            //            statement
            //            statement_list   statement_terminator    statement

            VerifyTerm(statementListNode, this._grammar.statement_list/* TODO: , named_block_list*/);

            // +2 because it alternaties statement/terminator
            for (int i = 0; i < statementListNode.ChildNodes.Count; i += 2)
            {
                yield return BuildStatementAst(statementListNode.ChildNodes[i]);
            }
        }

        StatementAst BuildStatementAst(ParseTreeNode statementNode)
        {
            ////        statement:
            ////            if_statement
            ////            label_opt   labeled_statement
            ////            function_statement
            ////            flow_control_statement   statement_terminator
            ////            trap_statement
            ////            try_statement
            ////            data_statement
            ////            pipeline   statement_terminator

            VerifyTerm(statementNode, this._grammar.statement);

            if (statementNode.ChildNodes[0].Term == this._grammar.pipeline)
            {
                return BuildPipelineAst(statementNode.ChildNodes[0]);
            }

            else throw new NotImplementedException(statementNode.ChildNodes[0].Term.Name);
        }

        PipelineBaseAst BuildPipelineAst(ParseTreeNode pipelineNode)
        {
            // alternative grammar:
            //          pipeline:
            //            assignment_expression
            //            _pipeline_expression
            //            _pipeline_command

            VerifyTerm(pipelineNode, this._grammar.pipeline);
            var childNode = pipelineNode.ChildNodes.Single();

            if (childNode.Term == this._grammar.assignment_expression)
            {
                throw new NotImplementedException(pipelineNode.ChildNodes[0].Term.Name);
            }

            else if (childNode.Term == this._grammar._pipeline_expression)
            {
                return BuildPipelineExpressionAst(childNode);
            }

            else if (childNode.Term == this._grammar._pipeline_command)
            {
                return BuildPipelineCommandAst(childNode);
            }

            else throw new InvalidOperationException(pipelineNode.ToString());
        }

        private PipelineBaseAst BuildPipelineExpressionAst(ParseTreeNode parseTreeNode)
        {
            //        _pipeline_expression:
            //            expression   redirections_opt  pipeline_tail_opt
            VerifyTerm(parseTreeNode, this._grammar._pipeline_expression);

            var commandExpressionAst = new CommandExpressionAst(
                new ScriptExtent(parseTreeNode.ChildNodes[0]),
                BuildExpressionAst(parseTreeNode.ChildNodes[0]), null
                );

            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return new PipelineAst(
                    new ScriptExtent(parseTreeNode),
                    commandExpressionAst
                    );
            }
            else if (parseTreeNode.ChildNodes.Count == 2 && parseTreeNode.ChildNodes[1].Term == this._grammar.pipeline_tail)
            {
                var pipelineTail = GetPipelineTailCommandList(parseTreeNode.ChildNodes[1]).ToList();
                pipelineTail.Insert(0, commandExpressionAst);
                return new PipelineAst(new ScriptExtent(parseTreeNode), pipelineTail);
            }

            else throw new NotImplementedException(parseTreeNode.ToString());

        }

        private PipelineBaseAst BuildPipelineCommandAst(ParseTreeNode parseTreeNode)
        {
            //        _pipeline_command:
            //            command   pipeline_tail_opt

            VerifyTerm(parseTreeNode, this._grammar._pipeline_command);

            CommandAst commandAst = BuildCommandAst(parseTreeNode.ChildNodes[0]);
            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return new PipelineAst(new ScriptExtent(parseTreeNode), commandAst);
            }
            else if (parseTreeNode.ChildNodes.Count == 2)
            {
                var pipelineTail = GetPipelineTailCommandList(parseTreeNode.ChildNodes[1]).ToList();
                pipelineTail.Insert(0, commandAst);
                return new PipelineAst(new ScriptExtent(parseTreeNode), pipelineTail);
            }
            else throw new InvalidOperationException(parseTreeNode.ToString());
        }

        IEnumerable<CommandBaseAst> GetPipelineTailCommandList(ParseTreeNode parseTreeNode)
        {
            ////        pipeline_tail:
            ////            |   new_lines_opt   command
            ////            |   new_lines_opt   command   pipeline_tail

            VerifyTerm(parseTreeNode, this._grammar.pipeline_tail);

            yield return BuildCommandAst(parseTreeNode.ChildNodes[1]);

            while (parseTreeNode.ChildNodes.Count == 3)
            {
                parseTreeNode = parseTreeNode.ChildNodes[2];
                VerifyTerm(parseTreeNode, this._grammar.pipeline_tail);
                yield return BuildCommandAst(parseTreeNode.ChildNodes[1]);
            }
        }

        ExpressionAst BuildExpressionAst(ParseTreeNode expressionNode)
        {
            ////        expression:
            ////            logical_expression
            VerifyTerm(expressionNode, this._grammar.expression);
            return BuildLogicalExpressionAst(expressionNode.ChildNodes.Single());
        }

        ExpressionAst BuildLogicalExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        logical_expression:
            ////            bitwise_expression
            ////            logical_expression   _and   new_lines_opt   bitwise_expression
            ////            logical_expression   _or   new_lines_opt   bitwise_expression
            ////            logical_expression   _xor   new_lines_opt   bitwise_expression
            if (parseTreeNode.ChildNodes[0].Term == this._grammar.bitwise_expression)
            {
                return BuildBitwiseExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildBitwiseExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        bitwise_expression:
            ////            comparison_expression
            ////            bitwise_expression   _band   new_lines_opt   comparison_expression
            ////            bitwise_expression   _bor   new_lines_opt   comparison_expression
            ////            bitwise_expression   _bxor   new_lines_opt   comparison_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.comparison_expression)
            {
                return BuildComparisonExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildComparisonExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        comparison_expression:
            ////            additive_expression
            ////            comparison_expression   comparison_operator   new_lines_opt   additive_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.additive_expression)
            {
                return BuildAdditiveExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildAdditiveExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        additive_expression:
            ////            multiplicative_expression
            ////            additive_expression   +   new_lines_opt   multiplicative_expression
            ////            additive_expression   dash   new_lines_opt   multiplicative_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.multiplicative_expression)
            {
                return BuildMultiplicativeExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else
            {
                var leftOperand = parseTreeNode.ChildNodes[0];
                var operatorNode = parseTreeNode.ChildNodes[1];
                var rightOperand = parseTreeNode.ChildNodes[2];

                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    BuildAdditiveExpressionAst(leftOperand),
                    operatorNode.Term == PowerShellGrammar.dash ? TokenKind.Minus : TokenKind.Plus,
                    BuildMultiplicativeExpressionAst(rightOperand),
                    new ScriptExtent(operatorNode)
                    );
            }
        }

        ExpressionAst BuildMultiplicativeExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        multiplicative_expression:
            ////            format_expression
            ////            multiplicative_expression   *   new_lines_opt   format_expression
            ////            multiplicative_expression   /   new_lines_opt   format_expression
            ////            multiplicative_expression   %   new_lines_opt   format_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.format_expression)
            {
                return BuildFormatExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildFormatExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        format_expression:
            ////            range_expression
            ////            format_expression   format_operator    new_lines_opt   range_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.range_expression)
            {
                return BuildRangeExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildRangeExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        range_expression:
            ////            array_literal_expression
            ////            range_expression   ..   new_lines_opt   array_literal_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.array_literal_expression)
            {
                return BuildArrayLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }
            else
            {
                var startNode = parseTreeNode.ChildNodes[0];
                var dotDotNode = parseTreeNode.ChildNodes[1];
                var endNode = parseTreeNode.ChildNodes[2];

                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    BuildRangeExpressionAst(startNode),
                    TokenKind.DotDot,
                    BuildArrayLiteralExpressionAst(endNode),
                    new ScriptExtent(dotDotNode)
                    );
            }
        }

        ExpressionAst BuildArrayLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        array_literal_expression:
            ////            unary_expression
            ////            unary_expression   ,    new_lines_opt   array_literal_expression

            VerifyTerm(parseTreeNode, this._grammar.array_literal_expression);
            VerifyTerm(parseTreeNode.ChildNodes[0], this._grammar.unary_expression);

            var unaryExpression = BuildUnaryExpressionAst(parseTreeNode.ChildNodes.First());

            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return unaryExpression;
            }
            else if (parseTreeNode.ChildNodes.Count == 3)
            {
                List<ExpressionAst> elements = new List<ExpressionAst>();
                elements.Add(unaryExpression);

                var remaining = BuildArrayLiteralExpressionAst(parseTreeNode.ChildNodes[2]);
                if (remaining is ArrayLiteralAst)
                {
                    elements.AddRange(((ArrayLiteralAst)remaining).Elements);
                }
                else
                {
                    elements.Add(remaining);
                }

                return new ArrayLiteralAst(new ScriptExtent(parseTreeNode), elements);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        ExpressionAst BuildUnaryExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        unary_expression:
            ////            primary_expression
            ////            expression_with_unary_operator

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.primary_expression)
            {
                return BuildPrimaryExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.expression_with_unary_operator)
            {
                return BuildExpressionWithUnaryOperatorAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        private ExpressionAst BuildExpressionWithUnaryOperatorAst(ParseTreeNode parseTreeNode)
        {
            ////        expression_with_unary_operator:
            ////            ,   new_lines_opt   unary_expression
            ////            -not   new_lines_opt   unary_expression
            ////            !   new_lines_opt   unary_expression
            ////            -bnot   new_lines_opt   unary_expression
            ////            +   new_lines_opt   unary_expression
            ////            dash   new_lines_opt   unary_expression
            ////            pre_increment_expression
            ////            pre_decrement_expression
            ////            cast_expression
            ////            -split   new_lines_opt   unary_expression
            ////            -join   new_lines_opt   unary_expression

            if (parseTreeNode.ChildNodes[0].Term == PowerShellGrammar.dash)
            {
                var expression = BuildUnaryExpressionAst(parseTreeNode.ChildNodes[1]);
                ConstantExpressionAst constantExpressionAst = expression as ConstantExpressionAst;
                if (constantExpressionAst == null)
                {
                    throw new NotImplementedException(parseTreeNode.ToString());
                }
                else
                {
                    if (constantExpressionAst.StaticType == typeof(int))
                    {
                        return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), 0 - ((int)constantExpressionAst.Value));
                    }
                    else
                    {
                        throw new NotImplementedException(parseTreeNode.ToString());
                    }
                }
            }

            throw new NotImplementedException(parseTreeNode.ToString());
        }

        ExpressionAst BuildPrimaryExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        primary_expression:
            ////            value
            ////            member_access
            ////            element_access
            ////            invocation_expression
            ////            post_increment_expression
            ////            post_decrement_expression

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.value)
            {
                return BuildValueExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildValueExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        value:
            ////            parenthesized_expression
            ////            sub_expression
            ////            array_expression
            ////            script_block_expression
            ////            hash_literal_expression
            ////            literal
            ////            type_literal
            ////            variable

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.literal)
            {
                return BuildLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else if (parseTreeNode.ChildNodes[0].Term == this._grammar.parenthesized_expression)
            {
                return BuildParenExpressionAst(parseTreeNode.ChildNodes[0]);
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        private ParenExpressionAst BuildParenExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        parenthesized_expression:
            ////            (   new_lines_opt   pipeline   new_lines_opt   )

            VerifyTerm(parseTreeNode, this._grammar.parenthesized_expression);

            return new ParenExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildPipelineAst(parseTreeNode.ChildNodes[1])
                );
        }

        ConstantExpressionAst BuildLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        literal:
            ////            integer_literal
            ////            real_literal
            ////            string_literal

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.integer_literal)
            {
                return BuildIntegerLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else if (parseTreeNode.ChildNodes[0].Term == this._grammar.string_literal)
            {
                return BuildStringLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ConstantExpressionAst BuildIntegerLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        integer_literal:
            ////            decimal_integer_literal
            ////            hexadecimal_integer_literal

            if (parseTreeNode.ChildNodes[0].Term == PowerShellGrammar.decimal_integer_literal)
            {
                return BuildDecimalIntegerLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else if (parseTreeNode.ChildNodes[0].Term == PowerShellGrammar.hexadecimal_integer_literal)
            {
                return BuildHexaecimalIntegerLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ConstantExpressionAst BuildDecimalIntegerLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        decimal_integer_literal:
            ////            decimal_digits   numeric_type_suffix_opt   numeric_multiplier_opt
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), PowerShellGrammar.decimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[PowerShellGrammar.decimal_digits.Name].Value;

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), Convert.ToInt32(value, 10));
        }

        ConstantExpressionAst BuildHexaecimalIntegerLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        hexadecimal_integer_literal:
            ////            0x   hexadecimal_digits   long_type_suffix_opt   numeric_multiplier_opt
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), PowerShellGrammar.hexadecimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[PowerShellGrammar.hexadecimal_digits.Name].Value;

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), Convert.ToInt32(value, 16));
        }

        StringConstantExpressionAst BuildStringLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        string_literal:
            ////            expandable_string_literal
            ////            expandable_here_string_literal
            ////            verbatim_string_literal
            ////            verbatim_here_string_literal

            VerifyTerm(parseTreeNode, this._grammar.string_literal);

            if (parseTreeNode.ChildNodes[0].Term == PowerShellGrammar.expandable_string_literal)
            {
                return BuildExpandableStringLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else if (parseTreeNode.ChildNodes[0].Term == PowerShellGrammar.verbatim_string_literal)
            {
                return BuildVerbatimStringLiteralExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        StringConstantExpressionAst BuildExpandableStringLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        expandable_string_literal:
            ////            double_quote_character   expandable_string_characters_opt   dollars_opt   double_quote_character
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), PowerShellGrammar.expandable_string_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[PowerShellGrammar.expandable_string_characters.Name].Value +
                matches.Groups[PowerShellGrammar.dollars.Name].Value
                ;

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.DoubleQuoted);
        }

        StringConstantExpressionAst BuildVerbatimStringLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        verbatim_string_literal:
            ////            single_quote_character   verbatim_string_characters_opt   single_quote_char [sic]

            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), PowerShellGrammar.verbatim_string_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[PowerShellGrammar.verbatim_string_characters.Name].Value;

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.SingleQuoted);
        }

        CommandAst BuildCommandAst(ParseTreeNode parseTreeNode)
        {
            ////        command:
            ////            command_name   command_elements_opt
            ////            command_invocation_operator   command_module_opt  command_name_expr   command_elements_opt

            VerifyTerm(parseTreeNode, this._grammar.command);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.command_name)
            {
                var commandElements = new List<CommandElementAst>();

                commandElements.Add(BuildCommandNameAst(parseTreeNode.ChildNodes[0]));

                if (parseTreeNode.ChildNodes.Count == 2)
                {
                    foreach (var commandElementNode in parseTreeNode.ChildNodes[1].ChildNodes)
                    {
                        commandElements.Add(BuildCommandElementAst(commandElementNode));
                    }
                }

                return new CommandAst(
                    new ScriptExtent(parseTreeNode),
                    commandElements,
                    TokenKind.Unknown,
                    null);

            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        private CommandElementAst BuildCommandNameAst(ParseTreeNode parseTreeNode)
        {
            ////        command_name:
            ////            generic_token
            ////            generic_token_with_subexpr

            VerifyTerm(parseTreeNode, this._grammar.command_name);

            if (parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.generic_token)
            {
                return BuildGenericTokenAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new NotImplementedException(this.ToString());
        }

        private StringConstantExpressionAst BuildGenericTokenAst(ParseTreeNode parseTreeNode)
        {
            ////        generic_token:
            ////            generic_token_parts

            VerifyTerm(parseTreeNode, PowerShellGrammar.generic_token);

            ////        generic_token_part:
            ////            expandable_string_literal
            ////            verbatim_here_string_literal
            ////            variable
            ////            generic_token_char

            // I'm confused by the idea that a generic_token could have several of these things smushed together, like this:
            //    PS> $x = "Get-"
            //    PS> $x"ChildItem"     # really? This gives an error in PowerShell. But:
            //    PS> & $x"ChildItem"   # works!
            //    PS> g"et-childite"m   # also works

            var match = PowerShellGrammar.generic_token.Expression.Match(parseTreeNode.Token.Text);

            if (match.Groups[PowerShellGrammar.expandable_string_literal.Name].Success) throw new NotImplementedException(parseTreeNode.ToString());
            //if (match.Groups[PowerShellGrammar.verbatim_here_string_literal.Name].Success) throw new NotImplementedException(parseTreeNode.ToString());
            if (match.Groups[PowerShellGrammar.variable.Name].Success) throw new NotImplementedException(parseTreeNode.ToString());

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), parseTreeNode.Token.Text, StringConstantType.BareWord);
        }

        private CommandElementAst BuildCommandElementAst(ParseTreeNode parseTreeNode)
        {
            ////        command_element:
            ////            command_parameter
            ////            command_argument
            ////            redirection

            VerifyTerm(parseTreeNode, this._grammar.command_element);

            var childNode = parseTreeNode.ChildNodes.Single();

            if (childNode.Term == PowerShellGrammar.command_parameter)
            {
                return BuildCommandParameterAst(childNode);
            }

            else if (childNode.Term == this._grammar.command_argument)
            {
                return BuildArgumentParameterAst(childNode);
            }

            if (childNode.Term == this._grammar.redirection) throw new NotImplementedException(childNode.ToString());

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        private CommandElementAst BuildArgumentParameterAst(ParseTreeNode parseTreeNode)
        {
            ////        command_argument:
            ////            command_name_expr

            VerifyTerm(parseTreeNode, this._grammar.command_argument);

            return BuildCommandNameExpressionAst(parseTreeNode.ChildNodes.Single());
        }

        private CommandElementAst BuildCommandNameExpressionAst(ParseTreeNode parseTreeNode)
        {
            ////        command_name_expr:
            ////            command_name
            ////            primary_expression

            VerifyTerm(parseTreeNode, this._grammar.command_name_expr);

            if (parseTreeNode.ChildNodes.Single().Term == this._grammar.command_name)
            {
                return BuildCommandNameAst(parseTreeNode.ChildNodes.Single());
            }

            else if (parseTreeNode.ChildNodes.Single().Term == this._grammar.primary_expression)
            {
                return BuildPrimaryExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else throw new InvalidOperationException(parseTreeNode.ToString());
        }

        private CommandParameterAst BuildCommandParameterAst(ParseTreeNode parseTreeNode)
        {
            ////        command_parameter:
            ////            dash   first_parameter_char   parameter_chars   colon_opt

            VerifyTerm(parseTreeNode, PowerShellGrammar.command_parameter);

            var match = PowerShellGrammar.command_parameter.Expression.Match(parseTreeNode.Token.Text);
            var parameterName = match.Groups[PowerShellGrammar._parameter_name.Name].Value;

            bool colon = match.Groups[PowerShellGrammar.colon.Name].Success;

            // to match PowerShell's behavior, we have to bump command_parameter to be a nonterminal. Later.
            // 
            // Try parsing this to see:
            //    x -y:$z
            //
            //    PS> $ast.EndBlock.Statements[0].PipelineElements[0].CommandElements[1]
            //
            //    ParameterName : y
            //    Argument      : $z
            //    ErrorPosition : -y:
            //    Extent        : -y:$z
            //    Parent        : x -y:$z

            if (colon) throw new NotImplementedException("can't parse colon parameters");

            return new CommandParameterAst(new ScriptExtent(parseTreeNode), parameterName, null, new ScriptExtent(parseTreeNode));
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Language;

using Irony.Parsing;
using Pash.ParserIntrinsics;
using System.Text.RegularExpressions;
using System.Reflection;

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

            if (!allExpected.Where(node => parseTreeNode.Term == node).Any())
            {
                throw new InvalidOperationException("expected '{0}' to be a '{1}'".FormatString(parseTreeNode, allExpected.JoinString(", ")));
            }
        }

        public ScriptBlockAst BuildScriptBlockAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.script_block);

            ParamBlockAst paramBlockAst = null;
            StatementBlockAst statementBlockAst = null;

            if (parseTreeNode.ChildNodes.Any())
            {
                // Note that I used First() and Last() to make it deal with the fact that both are optional
                if (parseTreeNode.ChildNodes.First().Term == this._grammar.param_block)
                {
                    paramBlockAst = BuildParamBlockAst(parseTreeNode.ChildNodes.First());
                }

                if (parseTreeNode.ChildNodes.Last().Term == this._grammar.script_block_body)
                {
                    statementBlockAst = BuildScriptBlockBodyAst(parseTreeNode.ChildNodes.Last());
                }
            }

            return new ScriptBlockAst(
                new ScriptExtent(parseTreeNode),
                paramBlockAst,
                statementBlockAst,
                false
                );
        }

        ParamBlockAst BuildParamBlockAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.param_block);

            IEnumerable<ParameterAst> parameters = BuildParameterListAst(parseTreeNode.ChildNodes[2]);

            return new ParamBlockAst(
                new ScriptExtent(parseTreeNode),
                new AttributeAst[0],
                parameters);
        }

        private IEnumerable<ParameterAst> BuildParameterListAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.parameter_list);

            return from ParseTreeNode parameter in parseTreeNode.ChildNodes
                   where parameter.Term == this._grammar.script_parameter
                   select BuildParameterAst(parameter);
        }

        private ParameterAst BuildParameterAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.script_parameter);

            ExpressionAst defaultValueExpression = null;

            if (parseTreeNode.ChildNodes.Last().Term == this._grammar.script_parameter_default)
            {
                defaultValueExpression = BuildParameterDefaultExpressionAst(parseTreeNode.ChildNodes.Last());
            }

            return new ParameterAst(
                new ScriptExtent(parseTreeNode),
                BuildVariableAst(parseTreeNode.ChildNodes[0]),
                new AttributeAst[0],
                defaultValueExpression);
        }

        private ExpressionAst BuildParameterDefaultExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.script_parameter_default);

            return BuildExpressionAst(parseTreeNode.ChildNodes[1]);
        }

        ScriptBlockExpressionAst BuildScriptBlockExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.script_block_expression);

            var scriptBlockNode = parseTreeNode.ChildNodes[1];

            return new ScriptBlockExpressionAst(
                new ScriptExtent(scriptBlockNode),
                BuildScriptBlockAst(scriptBlockNode)
                );
        }

        StatementBlockAst BuildScriptBlockBodyAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.script_block_body);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.named_block_list)
            {
                return BuildNamedBlockListAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.statement_list)
            {
                return BuildStatementListAst(parseTreeNode);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        StatementBlockAst BuildStatementListAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.statement_list);

            IEnumerable<StatementAst> statements = parseTreeNode
                .ChildNodes
                .Where(node => node.Term != this._grammar.statement_terminators)
                .Select(BuildStatementAst)
                ;

            return new StatementBlockAst(new ScriptExtent(parseTreeNode), statements, new TrapStatementAst[] { });
        }

        StatementBlockAst BuildNamedBlockListAst(ParseTreeNode parseTreeNode)
        {
            throw new NotImplementedException();
        }

        StatementAst BuildStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.statement);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.if_statement)
            {
                return BuildIfStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar._statement_labeled_statement)
            {
                return BuildLabeledStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.function_statement)
            {
                return BuildFunctionStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.flow_control_statement)
            {
                return BuildFlowControlStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.trap_statement)
            {
                return BuildTrapStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.try_statement)
            {
                return BuildTryStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.data_statement)
            {
                return BuildDataStatementAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.pipeline)
            {
                return BuildPipelineAst(parseTreeNode);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        StatementAst BuildFunctionStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.function_statement);

            var functionOrFilterTerm = parseTreeNode.ChildNodes[0];

            bool isFilter = functionOrFilterTerm.ChildNodes.Single().Token.Text == "filter";

            ScriptBlockAst scriptBlock = null;
            IEnumerable<ParameterAst> parameters = null;

            if (parseTreeNode.ChildNodes.Count == 6)
            {
                parameters = BuildParameterListAst(parseTreeNode.ChildNodes[2].ChildNodes[1]);
                scriptBlock = BuildScriptBlockAst(parseTreeNode.ChildNodes[4]);
            }
            else
            {
                scriptBlock = BuildScriptBlockAst(parseTreeNode.ChildNodes[3]);
            }

            return new FunctionDefinitionAst(
                new ScriptExtent(parseTreeNode),
                isFilter,
                false,
                parseTreeNode.ChildNodes[1].FindTokenAndGetText(),
                parameters,
                scriptBlock
                );
        }

        StatementAst BuildFlowControlStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.flow_control_statement);

            var childNode = parseTreeNode.ChildNodes.Single();
            if (childNode.Term == this._grammar._flow_control_statement_return)
            {
                return BuildReturnStatementAst(childNode);
            }

            else if (childNode.Term == this._grammar._flow_control_statement_exit)
            {
                return BuildExitStatementAst(childNode);
            }

            throw new NotImplementedException(childNode.ToString());
        }

        private StatementAst BuildReturnStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._flow_control_statement_return);

            PipelineBaseAst pipeline;

            if (parseTreeNode.ChildNodes.Count == 1)
            {
                pipeline = null;
            }

            else if (parseTreeNode.ChildNodes.Count == 2)
            {
                pipeline = BuildPipelineAst(parseTreeNode.ChildNodes[1]);
            }

            else
                throw new NotImplementedException(this.ToString());

            return new ReturnStatementAst(
                new ScriptExtent(parseTreeNode),
                pipeline
                );
        }

        private StatementAst BuildExitStatementAst(ParseTreeNode parseTreeNode)
        {
            return new ExitStatementAst(
                new ScriptExtent(parseTreeNode),
                null
                );
        }

        StatementAst BuildTrapStatementAst(ParseTreeNode parseTreeNode)
        {
            throw new NotImplementedException();
        }

        StatementAst BuildTryStatementAst(ParseTreeNode parseTreeNode)
        {
            return new TryStatementAst(
                new ScriptExtent(parseTreeNode),
                BuildStatementBlockAst(parseTreeNode.ChildNodes[0].ChildNodes[1]),
                BuildCatchClausesAst(parseTreeNode.ChildNodes[0].ChildNodes[2]),
                null
                );
        }

        private IEnumerable<CatchClauseAst> BuildCatchClausesAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.catch_clauses);

            return from ParseTreeNode catchClause in parseTreeNode.ChildNodes
                   where catchClause.Term == this._grammar.catch_clause
                   select BuildCatchClauseAst(catchClause);
        }

        private CatchClauseAst BuildCatchClauseAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.catch_clause);

            return new CatchClauseAst(
                new ScriptExtent(parseTreeNode),
                new TypeConstraintAst[0],
                BuildStatementBlockAst(parseTreeNode.ChildNodes[1])
                );
        }

        StatementAst BuildDataStatementAst(ParseTreeNode parseTreeNode)
        {
            throw new NotImplementedException();
        }

        StatementAst BuildLabeledStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._statement_labeled_statement);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            VerifyTerm(parseTreeNode, this._grammar.labeled_statement);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.for_statement)
            {
                return BuildForStatementAst(parseTreeNode);
            }

            else if (parseTreeNode.Term == this._grammar.foreach_statement)
            {
                return BuildForEachStatementAst(parseTreeNode);
            }

            throw new NotImplementedException();
        }

        ForStatementAst BuildForStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.for_statement);

            var initializerAst = BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[0]);
            var conditionAst = BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[1]);
            var iteratorAst = BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[2]);
            var bodyAst = BuildStatementBlockAst(parseTreeNode.ChildNodes[4]);

            return new ForStatementAst(
                new ScriptExtent(parseTreeNode),
                null,
                initializerAst,
                conditionAst,
                iteratorAst,
                bodyAst
                );
        }

        ForEachStatementAst BuildForEachStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.foreach_statement);

            var variableAst = BuildVariableAst(parseTreeNode.ChildNodes[2]);
            var enumerableExpression = BuildPipelineAst(parseTreeNode.ChildNodes[4]);
            var bodyAst = BuildStatementBlockAst(parseTreeNode.ChildNodes[6]);

            return new ForEachStatementAst(
                new ScriptExtent(parseTreeNode),
                null,
                ForEachFlags.None,
                variableAst,
                enumerableExpression,
                bodyAst
                );
        }

        IfStatementAst BuildIfStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.if_statement);

            var clauses = new List<Tuple<PipelineBaseAst, StatementBlockAst>>();

            clauses.Add(BuildIfStatementClauseAst(parseTreeNode.ChildNodes[1]));

            VerifyTerm(parseTreeNode.ChildNodes[2], this._grammar.elseif_clauses);

            var elseifNodes = parseTreeNode.ChildNodes[2].ChildNodes;

            clauses.AddRange(
                elseifNodes
                    .Select(n => BuildIfStatementClauseAst(n.ChildNodes[1]))
            );

            StatementBlockAst elseClause = null;

            if (parseTreeNode.ChildNodes.Last().Term == this._grammar.else_clause)
            {
                elseClause = BuildStatementBlockAst(parseTreeNode.ChildNodes.Last().ChildNodes[1]);
            }

            return new IfStatementAst(
                new ScriptExtent(parseTreeNode),
                clauses,
                elseClause
                );

            throw new NotImplementedException(parseTreeNode.ToString());
        }

        Tuple<PipelineBaseAst, StatementBlockAst> BuildIfStatementClauseAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._if_statement_clause);

            return new Tuple<PipelineBaseAst, StatementBlockAst>(
                            BuildIfStatementConditionAst(parseTreeNode.ChildNodes[0]),
                            BuildStatementBlockAst(parseTreeNode.ChildNodes[1])
                        );
        }


        PipelineBaseAst BuildIfStatementConditionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._if_statement_condition);
            return BuildPipelineAst(parseTreeNode.ChildNodes[1]);
        }

        StatementBlockAst BuildStatementBlockAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.statement_block);

            if (parseTreeNode.ChildNodes.Count == 2)
            {
                return new StatementBlockAst(
                    new ScriptExtent(parseTreeNode),
                    null,
                    null
                    );
            }
            else
            {
                return BuildStatementListAst(parseTreeNode.ChildNodes[1]);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        PipelineBaseAst BuildPipelineAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.pipeline);
            var childNode = parseTreeNode.ChildNodes.Single();

            if (childNode.Term == this._grammar.assignment_expression)
            {
                return BuildAssignementExpression(childNode);
            }

            if (childNode.Term == this._grammar._pipeline_expression)
            {
                return BuildPipelineExpressionAst(childNode);
            }

            if (childNode.Term == this._grammar._pipeline_command)
            {
                return BuildPipelineCommandAst(childNode);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        PipelineBaseAst BuildAssignementExpression(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.assignment_expression);

            var expressionAst = BuildPrimaryExpressionAst(parseTreeNode.ChildNodes[0]);
            TokenKind assignmentTokenKind = SelectTokenKind(parseTreeNode.ChildNodes[1]);
            var statementAst = BuildStatementAst(parseTreeNode.ChildNodes[2]);

            return new AssignmentStatementAst(
                new ScriptExtent(parseTreeNode),
                expressionAst,
                assignmentTokenKind,
                statementAst,
                new ScriptExtent(parseTreeNode.ChildNodes[1])
                );
        }

        TokenKind SelectTokenKind(ParseTreeNode parseTreeNode)
        {
            if (!(parseTreeNode.Term is Terminal)) throw new InvalidOperationException(parseTreeNode.ToString());

            var text = parseTreeNode.FindTokenAndGetText();

            switch (text)
            {
                case "=":
                    return TokenKind.Equals;

                default:
                    throw new NotImplementedException(parseTreeNode.ToString());
            }
        }

        PipelineBaseAst BuildPipelineExpressionAst(ParseTreeNode parseTreeNode)
        {
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

            if (parseTreeNode.ChildNodes.Count == 2 && parseTreeNode.ChildNodes[1].Term == this._grammar.pipeline_tails)
            {
                var pipelineTail = GetPipelineTailsCommandList(parseTreeNode.ChildNodes[1]).ToList();
                pipelineTail.Insert(0, commandExpressionAst);
                return new PipelineAst(new ScriptExtent(parseTreeNode), pipelineTail);
            }

            throw new NotImplementedException(parseTreeNode.ToString());

        }

        PipelineBaseAst BuildPipelineCommandAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._pipeline_command);

            CommandAst commandAst = BuildCommandAst(parseTreeNode.ChildNodes[0]);
            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return new PipelineAst(new ScriptExtent(parseTreeNode), commandAst);
            }
            if (parseTreeNode.ChildNodes.Count == 2)
            {
                var pipelineTail = GetPipelineTailsCommandList(parseTreeNode.ChildNodes[1]).ToList();
                pipelineTail.Insert(0, commandAst);
                return new PipelineAst(new ScriptExtent(parseTreeNode), pipelineTail);
            }
            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        IEnumerable<CommandBaseAst> GetPipelineTailsCommandList(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.pipeline_tails);

            return parseTreeNode.ChildNodes.Select(BuildPipelineTailAst).ToList();
        }

        ExpressionAst BuildExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.expression);

            return BuildLogicalExpressionAst(parseTreeNode.ChildNodes.Single());
        }

        ExpressionAst BuildLogicalExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.logical_expression);

            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return BuildBitwiseExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else
            {
                var leftAst = BuildLogicalExpressionAst(parseTreeNode.ChildNodes[0]);
                var binaryOperatorToken = GetBinaryOperatorTokenKind(parseTreeNode.ChildNodes[1]);
                var rightAst = BuildBitwiseExpressionAst(parseTreeNode.ChildNodes[2]);

                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    leftAst,
                    binaryOperatorToken,
                    rightAst,
                    new ScriptExtent(parseTreeNode.ChildNodes[1])
                    );
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildBitwiseExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.bitwise_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.comparison_expression)
            {
                return BuildComparisonExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildComparisonExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.comparison_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.additive_expression)
            {
                return BuildAdditiveExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            var comparisonOperatorNode = parseTreeNode.ChildNodes[1];

            return new BinaryExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildComparisonExpressionAst(parseTreeNode.ChildNodes[0]),
                GetBinaryOperatorTokenKind(comparisonOperatorNode),
                BuildAdditiveExpressionAst(parseTreeNode.ChildNodes[2]),
                new ScriptExtent(parseTreeNode.ChildNodes[1])
                );
        }

        TokenKind GetBinaryOperatorTokenKind(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode,
                this._grammar.comparison_operator,
                this._grammar._logical_expression_operator,
                this._grammar._bitwise_expression_operator,
                this._grammar._additive_expression_operator,
                this._grammar._multiplicative_expression_operator
                );

            var comparisonOperatorTerminal = (PowerShellGrammar.BinaryOperatorTerminal)(parseTreeNode.ChildNodes.Single().Term);

            return comparisonOperatorTerminal.TokenKind;
        }

        ExpressionAst BuildAdditiveExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.additive_expression);

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
                    operatorNode.Term == this._grammar.dash ? TokenKind.Minus : TokenKind.Plus,
                    BuildMultiplicativeExpressionAst(rightOperand),
                    new ScriptExtent(operatorNode)
                    );
            }
        }

        ExpressionAst BuildMultiplicativeExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.multiplicative_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.format_expression)
            {
                return BuildFormatExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildFormatExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.format_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.range_expression)
            {
                return BuildRangeExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildRangeExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.range_expression);

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
            VerifyTerm(parseTreeNode, this._grammar.array_literal_expression);
            VerifyTerm(parseTreeNode.ChildNodes[0], this._grammar.unary_expression);

            var unaryExpression = BuildUnaryExpressionAst(parseTreeNode.ChildNodes.First());

            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return unaryExpression;
            }
            if (parseTreeNode.ChildNodes.Count == 3)
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
            VerifyTerm(parseTreeNode, this._grammar.unary_expression);

            var childNode = parseTreeNode.ChildNodes.Single();

            if (childNode.Term == this._grammar.primary_expression)
            {
                return BuildPrimaryExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            if (childNode.Term == this._grammar.expression_with_unary_operator)
            {
                return BuildExpressionWithUnaryOperatorAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(childNode.Term.Name);
        }

        ExpressionAst BuildExpressionWithUnaryOperatorAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.expression_with_unary_operator);

            var childNode = parseTreeNode.ChildNodes.Single();

            if (childNode.Term == this._grammar._unary_dash_expression)
            {
                return BuildUnaryDashExpressionAst(childNode);
            }

            else if (childNode.Term == this._grammar.pre_increment_expression)
            {
                return BuildPreIncrementExpressionAst(childNode);
            }

            else if (childNode.Term == this._grammar.cast_expression)
            {
                return BuildCastExpression(childNode);
            }

            else if (childNode.Term == this._grammar._unary_not_expression)
            {
                return BuildUnaryNotExpressionAst(childNode);
            }

            throw new NotImplementedException(parseTreeNode.ToString());
        }

        ExpressionAst BuildCastExpression(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.cast_expression);

            return new ConvertExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildTypeConstraintAst(parseTreeNode.ChildNodes[0]),
                BuildUnaryExpressionAst(parseTreeNode.ChildNodes[1])
                );
        }

        TypeConstraintAst BuildTypeConstraintAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.type_literal);

            return new TypeConstraintAst(
                new ScriptExtent(parseTreeNode),
                BuildTypeName(parseTreeNode.ChildNodes[1].ChildNodes.Single())
                );
        }

        ExpressionAst BuildUnaryDashExpressionAst(ParseTreeNode parseTreeNode)
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

        ExpressionAst BuildPreIncrementExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.pre_increment_expression);

            return new UnaryExpressionAst(
                new ScriptExtent(parseTreeNode),
                TokenKind.PlusPlus,
                BuildUnaryExpressionAst(parseTreeNode.ChildNodes[1])
                );
        }

        ExpressionAst BuildUnaryNotExpressionAst(ParseTreeNode parseTreeNode)
        {
            return new UnaryExpressionAst(
                new ScriptExtent(parseTreeNode),
                TokenKind.Not,
                BuildUnaryExpressionAst(parseTreeNode.ChildNodes[1])
                );
        }

        ExpressionAst BuildPrimaryExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.primary_expression);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.value)
            {
                return BuildValueAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar._member_access_or_invocation_expression)
            {
                return BuildMemberAccessOrInvocationExpressionAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.element_access)
            {
                return BuildElementAccessAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.post_increment_expression)
            {
                return BuildPostIncrementExpressionAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.post_decrement_expression)
            {
                return BuildPostDecrementExpressionAst(parseTreeNode);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        ExpressionAst BuildPostDecrementExpressionAst(ParseTreeNode parseTreeNode)
        {
            throw new NotImplementedException();
        }

        ExpressionAst BuildPostIncrementExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.post_increment_expression);

            return new UnaryExpressionAst(
                new ScriptExtent(parseTreeNode),
                TokenKind.PostfixPlusPlus,
                BuildPrimaryExpressionAst(parseTreeNode.ChildNodes[0])
                );
        }

        IndexExpressionAst BuildElementAccessAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.element_access);

            var targetNode = parseTreeNode.ChildNodes[0];
            var indexNode = parseTreeNode.ChildNodes[2];

            return new IndexExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildPrimaryExpressionAst(targetNode),
                BuildExpressionAst(indexNode)
                );
        }

        MemberExpressionAst BuildMemberAccessOrInvocationExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._member_access_or_invocation_expression);

            var typeExpressionAst = BuildPrimaryExpressionAst(parseTreeNode.ChildNodes[0]);
            bool @static = parseTreeNode.ChildNodes[1].FindTokenAndGetText() == "::";
            var memberName = BuildMemberNameAst(parseTreeNode.ChildNodes[2]);

            if (parseTreeNode.ChildNodes.Count == 3)
            {
                return new MemberExpressionAst(new ScriptExtent(parseTreeNode), typeExpressionAst, memberName, @static);
            }

            else if (parseTreeNode.ChildNodes.Count == 5)
            {
                return new InvokeMemberExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    typeExpressionAst,
                    memberName,
                    null,
                    @static
                    );
            }

            else if (parseTreeNode.ChildNodes.Count == 6)
            {
                return new InvokeMemberExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    typeExpressionAst,
                    memberName,
                    BuildArgumentExpressionList(parseTreeNode.ChildNodes[4]),
                    @static
                    );
            }

            throw new NotImplementedException(parseTreeNode.ToString());
        }

        IEnumerable<ExpressionAst> BuildArgumentExpressionList(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.argument_expression_list);

            return parseTreeNode.ChildNodes.Select(BuildArgumentExpressionAst);
        }

        ExpressionAst BuildArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.argument_expression);

            return BuildLogicalArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
        }

        ExpressionAst BuildLogicalArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.logical_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.bitwise_argument_expression)
            {
                return BuildBitwiseArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildBitwiseArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.bitwise_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.comparison_argument_expression)
            {
                return BuildComparisonArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildComparisonArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.comparison_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.additive_argument_expression)
            {
                return BuildAdditiveArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            // This should probably call GetComparisonOperatorTokenKind
            var comparisonOperatorTerminal = (PowerShellGrammar.ComparisonOperatorTerminal)(parseTreeNode.ChildNodes[1].Term);

            return new BinaryExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildComparisonArgumentExpressionAst(parseTreeNode.ChildNodes[0]),
                comparisonOperatorTerminal.TokenKind,
                BuildAdditiveArgumentExpressionAst(parseTreeNode.ChildNodes[2]),
                new ScriptExtent(parseTreeNode.ChildNodes[1])
                );
        }

        ExpressionAst BuildAdditiveArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.additive_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.multiplicative_argument_expression)
            {
                return BuildMultiplicativeArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            else
            {
                var leftOperand = parseTreeNode.ChildNodes[0];
                var operatorNode = parseTreeNode.ChildNodes[1];
                var rightOperand = parseTreeNode.ChildNodes[2];

                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    BuildAdditiveArgumentExpressionAst(leftOperand),
                    operatorNode.Term == this._grammar.dash ? TokenKind.Minus : TokenKind.Plus,
                    BuildMultiplicativeArgumentExpressionAst(rightOperand),
                    new ScriptExtent(operatorNode)
                    );
            }
        }

        ExpressionAst BuildMultiplicativeArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.multiplicative_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.format_argument_expression)
            {
                return BuildFormatArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildFormatArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.format_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.range_argument_expression)
            {
                return BuildRangeArgumentExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ExpressionAst BuildRangeArgumentExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.range_argument_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.unary_expression)
            {
                return BuildUnaryExpressionAst(parseTreeNode.ChildNodes.Single());
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
                    BuildUnaryExpressionAst(endNode),
                    new ScriptExtent(dotDotNode)
                    );
            }
        }

        CommandElementAst BuildMemberNameAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.member_name);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.simple_name)
            {
                return BuildSimpleNameAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.string_literal)
            {
                return BuildStringLiteralAst(parseTreeNode);
            }


            if (parseTreeNode.Term == this._grammar.string_literal_with_subexpression)
            {
                return BuildStringLiteralWithSubexpressionAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.expression_with_unary_operator)
            {
                return BuildExpressionWithUnaryOperatorAst(parseTreeNode);
            }

            if (parseTreeNode.Term == this._grammar.value)
            {
                return BuildValueAst(parseTreeNode);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        CommandElementAst BuildStringLiteralWithSubexpressionAst(ParseTreeNode parseTreeNode)
        {
            throw new NotImplementedException(parseTreeNode.ToString());
        }

        ExpressionAst BuildValueAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.value);

            var childNode = parseTreeNode.ChildNodes.Single();

            if (childNode.Term == this._grammar.parenthesized_expression)
            {
                return BuildParenthesizedExpressionAst(childNode);
            }

            if (childNode.Term == this._grammar.sub_expression)
            {
                throw new NotImplementedException(childNode.Term.Name);
            }

            if (childNode.Term == this._grammar.array_expression)
            {
                return BuildArrayExpressionAst(childNode);
            }

            if (childNode.Term == this._grammar.script_block_expression)
            {
                return BuildScriptBlockExpressionAst(childNode);
            }

            if (childNode.Term == this._grammar.hash_literal_expression)
            {
                return BuildHashLiteralExpressionAst(childNode);
            }

            if (childNode.Term == this._grammar.literal)
            {
                return BuildLiteralAst(childNode);
            }

            if (childNode.Term == this._grammar.type_literal)
            {
                return BuildTypeLiteralAst(childNode);
            }

            if (childNode.Term == this._grammar.variable)
            {
                return BuildVariableAst(childNode);
            }

            throw new InvalidOperationException(childNode.Term.Name);
        }

        ArrayExpressionAst BuildArrayExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.array_expression);

            return new ArrayExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildStatementListAst(parseTreeNode.ChildNodes[1])
                );
        }

        VariableExpressionAst BuildVariableAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.variable);

            var match = this._grammar.variable.Expression.Match(parseTreeNode.Token.Text);

            if (match.Groups[this._grammar._variable_ordinary_variable.Name].Success)
            {
                return new VariableExpressionAst(new ScriptExtent(parseTreeNode), parseTreeNode.Token.Text.Substring(1), false);
            }

            throw new NotImplementedException(parseTreeNode.ToString());
        }

        TypeExpressionAst BuildTypeLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.type_literal);

            parseTreeNode = parseTreeNode.ChildNodes[1];


            VerifyTerm(parseTreeNode, this._grammar.type_spec);

            var firstNode = parseTreeNode.ChildNodes.First();

            var typeNameNode = parseTreeNode.ChildNodes.First();

            if (typeNameNode.Term == this._grammar.type_name)
            {
                return new TypeExpressionAst(new ScriptExtent(parseTreeNode), BuildTypeName(typeNameNode));
            }

            throw new NotImplementedException(typeNameNode.ToString());
        }

        TypeName BuildTypeName(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.type_name);

            return new TypeName(parseTreeNode.Token.Text);
        }

        HashtableAst BuildHashLiteralExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.hash_literal_expression);

            List<Tuple<ExpressionAst, StatementAst>> hashEntries = new List<Tuple<ExpressionAst, StatementAst>>();

            if (parseTreeNode.ChildNodes.Count == 3)
            {

                var hashLiteralBodyParseTreeNode = parseTreeNode.ChildNodes[1];


                VerifyTerm(hashLiteralBodyParseTreeNode, this._grammar.hash_literal_body);

                for (int i = 0; i < hashLiteralBodyParseTreeNode.ChildNodes.Count; i++)
                {
                    var hashEntryParseTreeNode = hashLiteralBodyParseTreeNode.ChildNodes[i];

                    if (hashEntryParseTreeNode.Term == _grammar.statement_terminators)
                    {
                        continue;
                    }

                    VerifyTerm(hashEntryParseTreeNode, this._grammar.hash_entry);

                    var keyExpressionAst = BuildKeyExpressionAst(hashEntryParseTreeNode.ChildNodes[0]);
                    var valueAst = BuildStatementAst(hashEntryParseTreeNode.ChildNodes[2]);

                    hashEntries.Add(new Tuple<ExpressionAst, StatementAst>(keyExpressionAst, valueAst));
                }
            }

            return new HashtableAst(new ScriptExtent(parseTreeNode), hashEntries);
        }

        ExpressionAst BuildKeyExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.key_expression);

            var childParseTreeNode = parseTreeNode.ChildNodes.Single();

            if (childParseTreeNode.Term == this._grammar.simple_name)
            {
                return BuildSimpleNameAst(childParseTreeNode);
            }

            if (childParseTreeNode.Term == this._grammar.unary_expression)
            {
                return BuildUnaryExpressionAst(childParseTreeNode);
            }

            throw new InvalidOperationException(childParseTreeNode.ToString());
        }

        ExpressionAst BuildSimpleNameAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.simple_name);
            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), parseTreeNode.Token.Text, StringConstantType.BareWord);
        }

        ParenExpressionAst BuildParenthesizedExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.parenthesized_expression);

            return new ParenExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildPipelineAst(parseTreeNode.ChildNodes[1])
                );
        }

        ConstantExpressionAst BuildLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.literal);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.integer_literal)
            {
                return BuildIntegerLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.string_literal)
            {
                return BuildStringLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ConstantExpressionAst BuildIntegerLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.integer_literal);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.decimal_integer_literal)
            {
                return BuildDecimalIntegerLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.hexadecimal_integer_literal)
            {
                return BuildHexaecimalIntegerLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        ConstantExpressionAst BuildDecimalIntegerLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.decimal_integer_literal);
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.decimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[this._grammar.decimal_digits.Name].Value;

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), Convert.ToInt32(value, 10));
        }

        ConstantExpressionAst BuildHexaecimalIntegerLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.hexadecimal_integer_literal);

            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.hexadecimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[this._grammar.hexadecimal_digits.Name].Value;

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), Convert.ToInt32(value, 16));
        }

        StringConstantExpressionAst BuildStringLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.string_literal);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.expandable_string_literal)
            {
                return BuildExpandableStringLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.verbatim_string_literal)
            {
                return BuildVerbatimStringLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        StringConstantExpressionAst BuildExpandableStringLiteralAst(ParseTreeNode parseTreeNode)
        {
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.expandable_string_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[this._grammar.expandable_string_characters.Name].Value +
                matches.Groups[this._grammar.dollars.Name].Value
                ;

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.DoubleQuoted);
        }

        StringConstantExpressionAst BuildVerbatimStringLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.verbatim_string_literal);

            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.verbatim_string_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[this._grammar.verbatim_string_characters.Name].Value;

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.SingleQuoted);
        }

        CommandAst BuildPipelineTailAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.pipeline_tail);

            return BuildCommandAst(parseTreeNode.ChildNodes[1]);
        }

        CommandAst BuildCommandAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            var commandElements = new List<CommandElementAst>();

            TokenKind invocationOperator = TokenKind.Unknown;

            if (parseTreeNode.Term == this._grammar._command_invocation)
            {
                VerifyTerm(parseTreeNode.ChildNodes[0], this._grammar.command_invocation_operator);

                switch (parseTreeNode.ChildNodes[0].FindTokenAndGetText())
                {
                    case "&":
                        invocationOperator = TokenKind.Ampersand;
                        break;

                    case ".":
                        invocationOperator = TokenKind.Dot;
                        break;

                    default:
                        throw new InvalidOperationException(parseTreeNode.ChildNodes[0].ToString());
                }

                // Issue: https://github.com/Pash-Project/Pash/issues/8 - command_module_opt is not in the grammar right now.

                commandElements.Add(BuildCommandNameExprAst(parseTreeNode.ChildNodes[1]));
            }

            else if (parseTreeNode.Term == this._grammar._command_simple)
            {
                commandElements.Add(BuildCommandNameAst(parseTreeNode.ChildNodes[0]));
            }

            else throw new InvalidOperationException(parseTreeNode.ChildNodes[0].Term.Name);

            ParseTreeNode commandElementsOptNode = parseTreeNode.ChildNodes.Last();

            if (commandElementsOptNode.ChildNodes.Any())
            {
                commandElements.AddRange(commandElementsOptNode.ChildNodes.Single().ChildNodes.Select(BuildCommandElementAst));
            }

            return new CommandAst(
                new ScriptExtent(parseTreeNode),
                commandElements,
                invocationOperator,
                null);

        }

        ExpressionAst BuildCommandNameExprAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_name_expr);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.command_name) return BuildCommandNameAst(parseTreeNode);

            if (parseTreeNode.Term == this._grammar.primary_expression) return BuildPrimaryExpressionAst(parseTreeNode);

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        StringConstantExpressionAst BuildCommandNameAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_name);

            if (parseTreeNode.ChildNodes.Single().Term == this._grammar.generic_token)
            {
                return BuildGenericTokenAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(this.ToString());
        }

        StringConstantExpressionAst BuildGenericTokenAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.generic_token);


            // I'm confused by the idea that a generic_token could have several of these things smushed together, like this:
            //    PS> g"et-childite"m   # OK!

            var match = this._grammar.generic_token.Expression.Match(parseTreeNode.Token.Text);

            if (match.Groups[this._grammar.expandable_string_literal.Name].Success) throw new NotImplementedException(parseTreeNode.ToString());
            //if (match.Groups[this._grammar.verbatim_here_string_literal.Name].Success) throw new NotImplementedException(parseTreeNode.ToString());
            if (match.Groups[this._grammar.variable.Name].Success) throw new NotImplementedException(parseTreeNode.ToString());

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), parseTreeNode.Token.Text, StringConstantType.BareWord);
        }

        CommandElementAst BuildCommandElementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_element);

            var childNode = parseTreeNode.ChildNodes.Single();

            if (childNode.Term == this._grammar.command_parameter)
            {
                return BuildCommandParameterAst(childNode);
            }

            if (childNode.Term == this._grammar.command_argument)
            {
                return BuildCommandArgumentAst(childNode);
            }

            if (childNode.Term == this._grammar.redirection) throw new NotImplementedException(childNode.ToString());

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        ExpressionAst BuildCommandArgumentAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_argument);

            var expressionAsts = parseTreeNode.ChildNodes.Select(BuildCommandNameExprAst);

            if (!expressionAsts.Any()) throw new InvalidOperationException();

            if (expressionAsts.Count() == 1) return expressionAsts.Single();

            else return new ArrayLiteralAst(new ScriptExtent(parseTreeNode), expressionAsts.ToList());
        }

        CommandParameterAst BuildCommandParameterAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_parameter);

            var match = this._grammar.command_parameter.Expression.Match(parseTreeNode.Token.Text);
            var parameterName = match.Groups[this._grammar._parameter_name.Name].Value;

            bool colon = match.Groups[this._grammar.colon.Name].Success;

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

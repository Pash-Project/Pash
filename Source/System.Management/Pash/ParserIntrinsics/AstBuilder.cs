// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;

using Irony.Parsing;
using Pash.ParserIntrinsics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Management.Automation;
using System.Linq.Expressions;
using System.Management.Pash.Implementation;

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
            // ISSUE: https://github.com/Pash-Project/Pash/issues/203
            // Since parameter_list was changed to parameter_list_opt we need
            // to anticipate a closing parenthesis here too.
            VerifyTerm(parseTreeNode, this._grammar.parameter_list, this._grammar.ToTerm(")"));

            var parameters = new List<ParameterAst>();
            var paramNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (ParseTreeNode curParam in parseTreeNode.ChildNodes)
            { 
                if (curParam.Term != this._grammar.script_parameter)
                {
                    continue;
                }
                var param = BuildParameterAst(curParam);
                var name = param.Name.VariablePath.UserPath;
                if (paramNames.Contains(name))
                {
                    var msg = String.Format("Duplicate parameter '{0}' in parameter list.", name);
                    throw new ParseException(msg);
                }
                parameters.Add(param);
                paramNames.Add(name);
            }
            return parameters;
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

            var statements = BuildStatementListRecursion(parseTreeNode);

            return new StatementBlockAst(
                new ScriptExtent(parseTreeNode),
                statements.Where(statement => !(statement is TrapStatementAst)),
                statements.OfType<TrapStatementAst>());
        }

        IEnumerable<StatementAst> BuildStatementListRecursion(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.statement_list, this._grammar._statement_list_rest);
            var numChildNodes = parseTreeNode.ChildNodes.Count;
            if (numChildNodes == 0)
            {
                return new StatementAst[0];
            }

            var statements = new List<StatementAst>();
            var childNode = parseTreeNode.ChildNodes.First();
            if (childNode.Term == this._grammar._unterminated_statement ||
                childNode.Term == this._grammar._terminated_statement)
            {
                statements.Add(BuildStatementAst(childNode));
            }
            else if (childNode.Term == this._grammar._statement_list_rest ||
                     childNode.Term == this._grammar.statement_list)
            {
                statements.AddRange(BuildStatementListRecursion(childNode));
            }

            // we can have more than one child to care about, then we have grammar rule recursion
            if (numChildNodes > 1 &&
                parseTreeNode.ChildNodes.Last().Term == this._grammar.statement_list)
            {
                statements.AddRange(BuildStatementListRecursion(parseTreeNode.ChildNodes.Last()));
            }
            return statements;
        }

        StatementBlockAst BuildNamedBlockListAst(ParseTreeNode parseTreeNode)
        {
            throw new NotImplementedException();
        }

        StatementAst BuildStatementAst(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.Term == this._grammar.statement)
            {
                parseTreeNode = parseTreeNode.ChildNodes.Single();
            }
            VerifyTerm(parseTreeNode, this._grammar._terminated_statement, this._grammar._unterminated_statement);

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

            if (parameters != null && scriptBlock.ParamBlock != null)
            {
                throw new ParseException("Cannot define both param block and parameters, use one of them");
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

            else if (childNode.Term == this._grammar._flow_control_statement_throw)
            {
                return BuildThrowStatementAst(childNode);
            }

            else if (childNode.Term == this._grammar._flow_control_statement_continue)
            {
                return BuildContinueStatementAst(childNode);
            }

            else if (childNode.Term == this._grammar._flow_control_statement_break)
            {
                return BuildBreakStatementAst(childNode);
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
            VerifyTerm(parseTreeNode, this._grammar._flow_control_statement_exit);

            var pipeline = parseTreeNode.ChildNodes.Count > 1 ? BuildPipelineAst(parseTreeNode.ChildNodes[1]) : null;
            return new ExitStatementAst(
                new ScriptExtent(parseTreeNode),
                pipeline
                );
        }

        private StatementAst BuildThrowStatementAst(ParseTreeNode parseTreeNode)
        {
            PipelineBaseAst pipeline = null;
            if (parseTreeNode.ChildNodes.Count == 2)
            {
                pipeline = BuildPipelineAst(parseTreeNode.ChildNodes[1]);
            }

            return new ThrowStatementAst(
                new ScriptExtent(parseTreeNode),
                pipeline
                );
        }

        StatementAst BuildTrapStatementAst(ParseTreeNode parseTreeNode)
        {
            TypeConstraintAst typeConstraint = null;
            StatementBlockAst statementBlock = null;

            if (parseTreeNode.ChildNodes.Count == 3)
            {
                typeConstraint = BuildTypeConstraintAst(parseTreeNode.ChildNodes[1]);
                statementBlock = BuildStatementBlockAst(parseTreeNode.ChildNodes[2]);
            }
            else
            {
                statementBlock = BuildStatementBlockAst(parseTreeNode.ChildNodes[1]);
            }

            return new TrapStatementAst(
                new ScriptExtent(parseTreeNode),
                typeConstraint,
                statementBlock);
        }

        private StatementAst BuildContinueStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._flow_control_statement_continue);
            var label = parseTreeNode.ChildNodes.Count < 2 ? null : BuildLabelExpressionAst(parseTreeNode.ChildNodes[1]);
            return new ContinueStatementAst(new ScriptExtent(parseTreeNode), label);
        }

        private StatementAst BuildBreakStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._flow_control_statement_break);
            var label = parseTreeNode.ChildNodes.Count < 2 ? null : BuildLabelExpressionAst(parseTreeNode.ChildNodes[1]);
            return new BreakStatementAst(new ScriptExtent(parseTreeNode), label);
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

            else if (parseTreeNode.Term == this._grammar.while_statement)
            {
                return BuildWhileStatementAst(parseTreeNode);
            }

            else if (parseTreeNode.Term == this._grammar.do_statement)
            {
                return BuildDoLoopStatementAst(parseTreeNode);
            }
            /*
            else if (parseTreeNode.Term == this._grammar.switch_statement)
            {
            }
            */

            throw new NotImplementedException("Switch statements are currently not supported");
        }

        ForStatementAst BuildForStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.for_statement);

            var headerAst = parseTreeNode.ChildNodes[2];

            var initializerAst =
                headerAst.ChildNodes.Count >= 1
                    ? BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[0])
                    : null;
            var conditionAst =
                headerAst.ChildNodes.Count >= 2
                    ? BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[1])
                    : null;
            var iteratorAst =
                headerAst.ChildNodes.Count == 3
                    ? BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[2])
                    : null;

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

        WhileStatementAst BuildWhileStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.while_statement);
            var conditionAst = BuildPipelineAst(parseTreeNode.ChildNodes[2].ChildNodes[0]);
            var bodyAst = BuildStatementBlockAst(parseTreeNode.ChildNodes[4]);

            return new WhileStatementAst(
                new ScriptExtent(parseTreeNode),
                null,
                conditionAst,
                bodyAst
                );
        }

        LoopStatementAst BuildDoLoopStatementAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.do_statement);
            var loopStatement = parseTreeNode.ChildNodes[0];
            if (loopStatement.ChildNodes.Count != 6)
            {
                throw new InvalidOperationException("Invalid do_statement. Please report this! " + this.ToString());
            }

            var body = BuildStatementBlockAst(loopStatement.ChildNodes[1]);
            var condition = BuildPipelineAst(loopStatement.ChildNodes[4].ChildNodes.Single());
            if (loopStatement.Term == this._grammar._do_statement_while)
            {
                return new DoWhileStatementAst(new ScriptExtent(parseTreeNode), null, condition, body);
            }
            else if (loopStatement.Term == this._grammar._do_statement_until)
            {
                return new DoUntilStatementAst(new ScriptExtent(parseTreeNode), null, condition, body);
            }

            throw new InvalidOperationException("Unknown do_statement. Please report this! " + this.ToString());
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

                case "+=":
                    return TokenKind.PlusEquals;

                case "-=":
                    return TokenKind.MinusEquals;

                case "*=":
                    return TokenKind.MultiplyEquals;

                case "/=":
                    return TokenKind.DivideEquals;

                case "%=":
                    return TokenKind.RemainderEquals;

                default:
                    throw new NotImplementedException(parseTreeNode.ToString());
            }
        }

        PipelineBaseAst BuildPipelineExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar._pipeline_expression);

            CommandExpressionAst commandExpressionAst = BuildCommandExpressionAst(parseTreeNode.ChildNodes[0]);

            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return new PipelineAst(
                    new ScriptExtent(parseTreeNode),
                    commandExpressionAst
                    );
            }

            ParseTreeNode pipeLineTailNode = null;
            if (parseTreeNode.ChildNodes.Count == 2 && parseTreeNode.ChildNodes[1].Term == this._grammar.pipeline_tails)
            {
                pipeLineTailNode = parseTreeNode.ChildNodes[1];
            }
            else if (parseTreeNode.ChildNodes.Count == 3 &&
                parseTreeNode.ChildNodes[1].Term == this._grammar.redirections &&
                parseTreeNode.ChildNodes[2].Term == this._grammar.pipeline_tails)
            {
                commandExpressionAst = BuildCommandExpressionAst(
                    parseTreeNode.ChildNodes[0],
                    BuildRedirectionsAst(parseTreeNode.ChildNodes[1])
                );
                pipeLineTailNode = parseTreeNode.ChildNodes[2];
            }
            else
            {
                throw new NotImplementedException(parseTreeNode.ToString());
            }

            var pipelineTail = GetPipelineTailsCommandList(pipeLineTailNode).ToList();
            pipelineTail.Insert(0, commandExpressionAst);
            return new PipelineAst(new ScriptExtent(parseTreeNode), pipelineTail);
        }

        private CommandExpressionAst BuildCommandExpressionAst(ParseTreeNode parseTreeNode, IEnumerable<RedirectionAst> redirections = null)
        {
            return new CommandExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildExpressionAst(parseTreeNode),
                redirections
            );
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
        }

        ExpressionAst BuildBitwiseExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.bitwise_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.comparison_expression)
            {
                return BuildComparisonExpressionAst(parseTreeNode.ChildNodes.Single());
            }
            else if (parseTreeNode.ChildNodes[0].Term == this._grammar.bitwise_expression)
            {
                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    BuildBitwiseExpressionAst(parseTreeNode.ChildNodes[0]),
                    GetBinaryOperatorTokenKind(parseTreeNode.ChildNodes[1]),
                    BuildComparisonExpressionAst(parseTreeNode.ChildNodes[2]),
                    new ScriptExtent(parseTreeNode.ChildNodes[1])
                    );
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
                // this._grammar._additive_expression_operator,
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
                    operatorNode.ChildNodes.Single().Term == this._grammar.dash ? TokenKind.Minus : TokenKind.Plus,
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
            else
            {
                var leftOperand = parseTreeNode.ChildNodes[0];
                var operatorNode = parseTreeNode.ChildNodes[1];
                var rightOperand = parseTreeNode.ChildNodes[2];

                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    BuildMultiplicativeExpressionAst(leftOperand),
                    ParseMultiplicativeOperator(operatorNode),
                    BuildFormatExpressionAst(rightOperand),
                    new ScriptExtent(operatorNode)
                    );
            }
        }

        ExpressionAst BuildFormatExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.format_expression);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.range_expression)
            {
                return BuildRangeExpressionAst(parseTreeNode.ChildNodes.Single());
            }

            var leftOperand = parseTreeNode.ChildNodes[0];
            var operatorNode = parseTreeNode.ChildNodes[1];
            var rightOperand = parseTreeNode.ChildNodes[2];

            return new BinaryExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildFormatExpressionAst(leftOperand),
                TokenKind.Format,
                BuildRangeExpressionAst(rightOperand),
                new ScriptExtent(operatorNode));
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
                // To avoid mistakes in recursion with ArrayLiteralAsts as last array element, we need to repack it
                if (unaryExpression is ArrayLiteralAst)
                {
                    return new ArrayLiteralAst(new ScriptExtent(parseTreeNode), new [] { unaryExpression });
                }
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
            // See Pash issue #214
            var subNode = parseTreeNode.ChildNodes.Single();
            // cast expression is not part of the joined expression, or double cast
            // like [string][int]"5" wouldn't work for any reason
            if (subNode.Term == this._grammar.cast_expression)
            {
                return BuildCastExpression(subNode);
            }
            // otherwise it's a joined unary operator expression
            VerifyTerm(subNode, this._grammar._joined_unary_operator_expression);

            var operatorTerm = subNode.ChildNodes[0].ChildNodes.Single().Term;
            var operatorKeyTerm = operatorTerm as KeyTerm;

            if (operatorTerm == this._grammar.dash)
            {
                return BuildUnaryDashExpressionAst(subNode);
            }
            else if (operatorTerm == this._grammar.type_literal) /* cast expression */
            {
                return BuildCastExpression(subNode);
            }
            else if (operatorTerm == this._grammar._operator_not ||
                     (operatorKeyTerm != null && operatorKeyTerm.Text.Equals("!")))
            {
                // bang expression is alternative spelling for -not
                return new UnaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    TokenKind.Not,
                    BuildUnaryExpressionAst(subNode.ChildNodes[1])
                    );
            }
            else if (operatorKeyTerm != null && operatorKeyTerm.Text.Equals("++"))
            {
                return BuildPrefixIncrementDecrementExpressionAst(subNode, "++", TokenKind.PlusPlus);
            }
            else if (operatorTerm == this._grammar.dashdash)
            {
                return BuildPrefixIncrementDecrementExpressionAst(subNode, "--", TokenKind.MinusMinus);
            }
            else if (operatorKeyTerm != null && operatorKeyTerm.Text.Equals(",")) //unary array
            {
                var unaryExpression = BuildUnaryExpressionAst(subNode.ChildNodes[1]);
                return new ArrayLiteralAst(new ScriptExtent(subNode), new [] {unaryExpression});
            }
            /* left to be implemented:
             * operatorTerm == _operator_bnot
             * operatorKeyTerm != null && operatorKeyTerm.Text.Equals("+") //unary plus
             * operatorKeyTerm != null && operatorKeyTerm.Text.Equals("-split") //split array
             * operatorKeyTerm != null && operatorKeyTerm.Text.Equals("-join") //join array
             */

            throw new NotImplementedException(subNode.ToString());
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
            var constantExpressionAst = expression as ConstantExpressionAst;
            var type = constantExpressionAst.StaticType;
            if (constantExpressionAst == null)
            {
                throw new NotImplementedException(parseTreeNode.ToString());
            }
            else
            {
                // From the spec (2.3.5.1.1 Integer literals):

                // “In the twos-complement representation of integer values,
                // there is one more negative value than there is positive.
                // For the int type, that extra value is -2147483648. For the
                // long type, that extra value is -9223372036854775808. Even
                // though the token 2147483648 would ordinarily be treated as
                // a literal of type long, if it is preceded immediately by
                // the unary - operator, that operator and literal are treated
                // as a literal of type int having the smallest value.
                // Similarly, even though the token 9223372036854775808 would
                // ordinarily be treated as a real literal of type decimal,
                // if it is immediately preceded by the unary - operator, that
                // operator and literal are treated as a literal of type long
                // having the smallest value.”

                // The following code deals both with conflating unary minus
                // and a constant expression into a single constant expression
                // as well as the type juggling the above quote from the
                // specification entails.

                // TODO: The wording of the specification only refers to
                // int.MinValue and long.MinValue, which should be parsed as a
                // single literal. Would we normally have to treat everything
                // else as the combination of an unary minus and a constant
                // expression?

                if (type == typeof(int))
                {
                    var value = (int)constantExpressionAst.Value;
                    return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), 0 - value);
                }
                else if (type == typeof(long))
                {
                    var value = (long)constantExpressionAst.Value;
                    if (value == -(long)int.MinValue)
                        return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), int.MinValue);
                    else
                        return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), 0L - value);
                }
                else if (type == typeof(decimal))
                {
                    var value = (decimal)constantExpressionAst.Value;
                    if (value == -(decimal)long.MinValue)
                        return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), long.MinValue);
                    else
                        return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), 0m - value);
                }
                else if (type == typeof(double))
                {
                    var value = (double)constantExpressionAst.Value;
                    return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), 0d - value);
                }
                else
                {
                    throw new NotImplementedException(parseTreeNode.ToString());
                }
            }
        }

        ExpressionAst BuildPrefixIncrementDecrementExpressionAst(ParseTreeNode parseTreeNode, string op, TokenKind token)
        {
            var unaryExpression = BuildUnaryExpressionAst(parseTreeNode.ChildNodes[1]);
            VerifyExpressionIsIncrementableOrDecrementable(unaryExpression, "++");
            return new UnaryExpressionAst(new ScriptExtent(parseTreeNode), token, unaryExpression);
        }

        ExpressionAst BuildPrimaryExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.primary_expression);

            parseTreeNode = parseTreeNode.ChildNodes.Single();

            if (parseTreeNode.Term == this._grammar.value)
            {
                return BuildValueAst(parseTreeNode);
            }

            else if (parseTreeNode.Term == this._grammar._member_access_or_invocation_expression)
            {
                return BuildMemberAccessOrInvocationExpressionAst(parseTreeNode);
            }

            else if (parseTreeNode.Term == this._grammar.element_access)
            {
                return BuildElementAccessAst(parseTreeNode);
            }

            else if (parseTreeNode.Term == this._grammar.post_increment_expression)
            {
                return BuildPostIncrementExpressionAst(parseTreeNode);
            }

            else if (parseTreeNode.Term == this._grammar.post_decrement_expression)
            {
                return BuildPostDecrementExpressionAst(parseTreeNode);
            }

            throw new InvalidOperationException(parseTreeNode.ToString());
        }

        ExpressionAst BuildPostDecrementExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.post_decrement_expression);
            var exp = BuildPrimaryExpressionAst(parseTreeNode.ChildNodes[0]);
            VerifyExpressionIsIncrementableOrDecrementable(exp, "--");
            return new UnaryExpressionAst(new ScriptExtent(parseTreeNode), TokenKind.PostfixMinusMinus, exp);
        }

        ExpressionAst BuildPostIncrementExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.post_increment_expression);
            var exp = BuildPrimaryExpressionAst(parseTreeNode.ChildNodes[0]);
            VerifyExpressionIsIncrementableOrDecrementable(exp, "++");
            return new UnaryExpressionAst(new ScriptExtent(parseTreeNode), TokenKind.PostfixPlusPlus, exp);
        }

        private void VerifyExpressionIsIncrementableOrDecrementable(ExpressionAst exp, string op)
        {
            if (!SettableExpression.SupportedExpressions.Contains(exp.GetType()))
            {
                var msg = String.Format("The operator '{0}' can only be used for variables or properties", op);
                throw new ParseException(msg);
            }
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
            else if (parseTreeNode.ChildNodes[0].Term == this._grammar.bitwise_argument_expression)
            {
                return new BinaryExpressionAst(
                   new ScriptExtent(parseTreeNode),
                   BuildBitwiseArgumentExpressionAst(parseTreeNode.ChildNodes[0]),
                   GetBinaryOperatorTokenKind(parseTreeNode.ChildNodes[1]),
                   BuildComparisonArgumentExpressionAst(parseTreeNode.ChildNodes[2]),
                   new ScriptExtent(parseTreeNode.ChildNodes[1])
                   );
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
                    operatorNode.ChildNodes.Single().Term == this._grammar.dash ? TokenKind.Minus : TokenKind.Plus,
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
            else
            {
                var leftOperand = parseTreeNode.ChildNodes[0];
                var operatorNode = parseTreeNode.ChildNodes[1];
                var rightOperand = parseTreeNode.ChildNodes[2];

                return new BinaryExpressionAst(
                    new ScriptExtent(parseTreeNode),
                    BuildMultiplicativeArgumentExpressionAst(leftOperand),
                    ParseMultiplicativeOperator(operatorNode),
                    BuildFormatArgumentExpressionAst(rightOperand),
                    new ScriptExtent(operatorNode)
                    );
            }
        }

        private TokenKind ParseMultiplicativeOperator(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, _grammar._multiplicative_expression_operator);
            KeyTerm keyTerm = (KeyTerm) parseTreeNode.ChildNodes.Single().Term as KeyTerm;
            if (keyTerm != null)
            {
                string symbol = keyTerm.Text;
                if (symbol.Equals("*"))
                {
                    return TokenKind.Multiply;
                }
                else if (symbol.Equals("/"))
                {
                    return TokenKind.Divide;
                }
                else if (symbol.Equals("%"))
                {
                    return TokenKind.Rem;
                }
            }
            throw new NotSupportedException(String.Format("Unsupported operator node '{0}'",
                                                          parseTreeNode.ToString()));
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
                return BuildSubExpression(childNode);
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

        ExpressionAst BuildSubExpression(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.sub_expression);

            return new SubExpressionAst(
                new ScriptExtent(parseTreeNode),
                BuildStatementListAst(parseTreeNode.ChildNodes[1]));
        }

        ArrayExpressionAst BuildArrayExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.array_expression);
            StatementBlockAst statements;
            // check if we have statements or it's empty
            if (parseTreeNode.ChildNodes[1].Term == _grammar.statement_list)
            {
                statements = BuildStatementListAst(parseTreeNode.ChildNodes[1]);
            }
            else
            {
                // otherwise make such an Ast without statements
                statements = new StatementBlockAst(new ScriptExtent(parseTreeNode.ChildNodes[1]),
                                                   new StatementAst[] { }, new TrapStatementAst[] { });
            }
            return new ArrayExpressionAst(
                new ScriptExtent(parseTreeNode),
                statements
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
            else if (match.Groups[this._grammar._variable_dollar_question.Name].Success)
            {
                return new VariableExpressionAst(new ScriptExtent(parseTreeNode), "?", false);
            }
            throw new NotImplementedException(parseTreeNode.ToString());
        }

        TypeExpressionAst BuildTypeLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.type_literal);

            parseTreeNode = parseTreeNode.ChildNodes[1];


            VerifyTerm(parseTreeNode, this._grammar.type_spec);

            var typeNameNode = parseTreeNode.ChildNodes.First();

            if (typeNameNode.Term == this._grammar.type_name)
            {
                return new TypeExpressionAst(new ScriptExtent(parseTreeNode), BuildTypeName(typeNameNode));
            }

            throw new NotImplementedException(typeNameNode.ToString());
        }

        TypeName BuildTypeName(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.type_name, _grammar._type_spec_array);
            int dimensions = 0;
            string name;
            if (parseTreeNode.Term == _grammar._type_spec_array)
            {
                name = parseTreeNode.ChildNodes[0].Token.Text;
                name = name.Substring(0, name.Length - 1); // omit "["
                dimensions = parseTreeNode.ChildNodes[1].ChildNodes.Count + 1; // one dimension + 1 for each comma
            }
            else // usual type name
            {
                name = parseTreeNode.Token.Text;
            }
            return new TypeName(name, dimensions);
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
            return BuildUnaryOrSimpleNameExpressionAstUnsafe(parseTreeNode);
        }

        ExpressionAst BuildLabelExpressionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.label_expression);
            return BuildUnaryOrSimpleNameExpressionAstUnsafe(parseTreeNode);
        }

        ExpressionAst BuildUnaryOrSimpleNameExpressionAstUnsafe(ParseTreeNode parseTreeNode)
        {
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

        ExpressionAst BuildLiteralAst(ParseTreeNode parseTreeNode)
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

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.real_literal)
            {
                return BuildRealLiteralAst(parseTreeNode.ChildNodes.Single());
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
                return BuildHexadecimalIntegerLiteralAst(parseTreeNode.ChildNodes.Single());
            }

            throw new NotImplementedException(parseTreeNode.ChildNodes[0].Term.Name);
        }

        private bool ParseIntOrLongLiteral(string literal, string multiplier, NumberStyles style,
            out object parsed)
        {
            int intValue;
            long longValue;

            // Note: TryParse will only ever return false here if the conversion overflows because
            // all other conditions are impossible when the supplied string consists only of digits.
            if (int.TryParse(literal, style, CultureInfo.InvariantCulture, out intValue))
            {
                parsed = NumericMultiplier.Multiply(intValue, multiplier);
                return true;
            }

            if (long.TryParse(literal, style, CultureInfo.InvariantCulture, out longValue))
            {
                parsed = NumericMultiplier.Multiply(longValue, multiplier);
                return true;
            }

            parsed = null;
            return false;
        }

        ConstantExpressionAst BuildDecimalIntegerLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.decimal_integer_literal);
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.decimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            string digits = matches.Groups[this._grammar.decimal_digits.Name].Value;
            string typeSuffix = matches.Groups[this._grammar.numeric_type_suffix.Name].Value;
            string longTypeSuffix = matches.Groups[this._grammar.long_type_suffix.Name].Value;
            string multiplier = matches.Groups[this._grammar.numeric_multiplier.Name].Value;

            // The type of an integer literal is determined by its value, the presence or absence of long-type-suffix, and the
            // presence of a numeric-multiplier (§2.3.5.1.3).

            object value;

            if (typeSuffix == string.Empty)
            {
                // For an integer literal with no long-type-suffix
                // • If its value can be represented by type int (§4.2.3), that is its type;
                // • Otherwise, if its value can be represented by type long (§4.2.3), that is its type.
                if (!ParseIntOrLongLiteral(digits, multiplier, NumberStyles.Integer, out value))
                {
                    decimal decimalValue;
                    double doubleValue;
                    if (decimal.TryParse(digits, out decimalValue))
                        // • Otherwise, if its value can be represented by type decimal (§2.3.5.1.2), that is its type.
                        value = NumericMultiplier.Multiply(decimalValue, multiplier);
                    else if (double.TryParse(digits, out doubleValue))
                        // • Otherwise, it is represented by type double (§2.3.5.1.2).
                        value = NumericMultiplier.Multiply(doubleValue, multiplier);
                    else
                        // For PowerShell compatibility throw an error here instead of saturating the double to infinity.
                        throw new OverflowException(string.Format("The integer literal {0} is too large.", matches.Value));
                }
            }
            else if (longTypeSuffix != string.Empty)
            {
                // For an integer literal with long-type-suffix

                long longValue;

                // • If its value can be represented by type long (§4.2.3), that is its type;
                if (long.TryParse(digits, out longValue))
                    value = NumericMultiplier.Multiply(longValue, multiplier);
                else
                    // • Otherwise, that literal is ill formed.
                    throw new ArithmeticException(string.Format("The integer literal {0} is invalid because it does not fit into a long.", matches.Value));
            }
            else
            {
                // The spec doesn't explicitly mention this case, but it seems to be handled
                // similar to a long suffix.
                decimal decimalValue;

                if (decimal.TryParse(digits, out decimalValue))
                    value = NumericMultiplier.Multiply(decimalValue, multiplier);
                else
                    throw new ArithmeticException(string.Format("The integer literal {0} is invalid because it does not fit into a decimal.", matches.Value));
            }

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), value);
        }

        ConstantExpressionAst BuildHexadecimalIntegerLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.hexadecimal_integer_literal);

            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.hexadecimal_integer_literal.Pattern, RegexOptions.IgnoreCase);
            string digits = matches.Groups[this._grammar.hexadecimal_digits.Name].Value;
            string multiplier = matches.Groups[this._grammar.numeric_multiplier.Name].Value;

            object value;
            // Hex literals can only be int or long.
            if (!ParseIntOrLongLiteral(digits, multiplier, NumberStyles.AllowHexSpecifier, out value))
                throw new OverflowException(string.Format("The integer literal {0} is too large.", matches.Value));

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), value);
        }

        ExpressionAst BuildStringLiteralAst(ParseTreeNode parseTreeNode)
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

        ExpressionAst BuildExpandableStringLiteralAst(ParseTreeNode parseTreeNode)
        {
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.expandable_string_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[this._grammar.expandable_string_characters.Name].Value +
                matches.Groups[this._grammar.dollars.Name].Value
                ;

            var ast = new ExpandableStringExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.DoubleQuoted);
            if (ast.NestedExpressions.Any())
            {
                return ast;
            }
            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.DoubleQuoted);
        }

        StringConstantExpressionAst BuildVerbatimStringLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.verbatim_string_literal);

            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.verbatim_string_literal.Pattern, RegexOptions.IgnoreCase);
            string value = matches.Groups[this._grammar.verbatim_string_characters.Name].Value;

            return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode), value, StringConstantType.SingleQuoted);
        }

        ConstantExpressionAst BuildRealLiteralAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.real_literal);
            var matches = Regex.Match(parseTreeNode.FindTokenAndGetText(), this._grammar.real_literal.Pattern, RegexOptions.IgnoreCase);
            Group multiplier = matches.Groups[this._grammar.numeric_multiplier.Name];
            Group decimalTypeSuffix = matches.Groups[this._grammar.decimal_type_suffix.Name];

            if (decimalTypeSuffix.Success)
            {
                return BuildDecimalRealLiteralAst(parseTreeNode, multiplier, decimalTypeSuffix);
            }

            return BuildDoubleRealLiteralAst(parseTreeNode, multiplier);
        }

        private ConstantExpressionAst BuildDecimalRealLiteralAst(ParseTreeNode parseTreeNode, Group multiplier, Group decimalTypeSuffix)
        {
            string digits = parseTreeNode.FindTokenAndGetText();

            if (multiplier.Success)
            {
                digits = RemoveMatchedString(digits, multiplier);
            }

            digits = RemoveMatchedString(digits, decimalTypeSuffix);

            decimal value;
            if (!decimal.TryParse(digits, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
            {
                throw new OverflowException(string.Format("Bad numeric constant: {0}.", parseTreeNode.FindTokenAndGetText()));
            }

            if (multiplier.Success)
            {
                value *= NumericMultiplier.GetValue(multiplier.Value);
            }

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), value);
        }

        private string RemoveMatchedString(string text, Group match)
        {
            return text.Substring(0, match.Index) + text.Substring(match.Index + match.Length);
        }

        private ConstantExpressionAst BuildDoubleRealLiteralAst(ParseTreeNode parseTreeNode, Group multiplier)
        {
            string digits = parseTreeNode.FindTokenAndGetText();

            if (multiplier.Success)
            {
                digits = RemoveMatchedString(digits, multiplier);
            }

            double value;
            if (!double.TryParse(digits, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
            {
                throw new OverflowException(string.Format("The real literal {0} is too large.", digits));
            }

            if (multiplier.Success)
            {
                value *= NumericMultiplier.GetValue(multiplier.Value);
            }

            return new ConstantExpressionAst(new ScriptExtent(parseTreeNode), value);
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
            List<RedirectionAst> redirections = null;

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
                commandElements.AddRange(BuildCommandOptElementAsts(commandElementsOptNode.ChildNodes.Single()));
                redirections = BuildCommandElementRedirectionAsts(commandElementsOptNode.ChildNodes.Single()).ToList();
            }

            return new CommandAst(
                new ScriptExtent(parseTreeNode),
                commandElements,
                invocationOperator,
                redirections);

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

        IEnumerable<CommandElementAst> BuildCommandOptElementAsts(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_elements);

            var elementAsts = new List<CommandElementAst>();
            CommandParameterAst lastParameter = null;

            var commandElements = from child in parseTreeNode.ChildNodes select child.ChildNodes.Single();

            foreach (var cmdElementTree in commandElements)
            {
                // first check if a parameter is waiting for its argument
                if (lastParameter != null && lastParameter.RequiresArgument)
                {
                    // lastParameter is already in elementAsts list, only set the Argument property
                    lastParameter.Argument = BuildCommandArgumentAstFromCommandElement(cmdElementTree,
                                                                                       lastParameter.ParameterName);
                    lastParameter = null; // makes sure we don't set it twice
                    continue;
                }

                // otherwise check for parameters, arguments, and redirection
                if (cmdElementTree.Term == this._grammar.command_parameter)
                {
                    lastParameter = BuildCommandParameterAst(cmdElementTree);
                    elementAsts.Add(lastParameter);
                }
                else if (cmdElementTree.Term == this._grammar.command_argument)
                {
                    elementAsts.Add(BuildCommandArgumentAst(cmdElementTree));
                }
                else if (cmdElementTree.Term == this._grammar.redirection)
                {
                    // Redirections are built separately.
                }
                else
                {
                    throw new InvalidOperationException(parseTreeNode.ToString());
                }
            }

            // make sure the last parameter didn't expect an argument that was not provided
            if (lastParameter != null && lastParameter.RequiresArgument)
            {
                throw new ParseException(String.Format("The parameter \"-{0}\" requires an argument",
                                                       lastParameter.ParameterName));
            }

            return elementAsts;
        }

        ExpressionAst BuildCommandArgumentAstFromCommandElement(ParseTreeNode parseTreeNode, string parameterName)
        {
            if (parseTreeNode.Term == this._grammar.command_parameter)
            {
                return new StringConstantExpressionAst(new ScriptExtent(parseTreeNode),
                                                       parseTreeNode.Token.Text, StringConstantType.BareWord);
            }
            else if (parseTreeNode.Term == this._grammar.command_argument)
            {
                return BuildCommandArgumentAst(parseTreeNode);
            }

            throw new ParseException(String.Format("The parameter \"-{0}\" requires an argument", parameterName));
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

            // colon at the end means that the next term will be the argument
            bool requiresArgument = match.Groups[this._grammar.colon.Name].Success;

            return new CommandParameterAst(new ScriptExtent(parseTreeNode), parameterName, null,
                                           new ScriptExtent(parseTreeNode), requiresArgument);
        }

        IEnumerable<RedirectionAst> BuildCommandElementRedirectionAsts(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.command_elements);

            var redirectionAsts = new List<RedirectionAst>();

            var commandElements = from child in parseTreeNode.ChildNodes select child.ChildNodes.Single();

            foreach (var cmdElementTree in commandElements)
            {
                if (cmdElementTree.Term == this._grammar.redirection)
                {
                    redirectionAsts.Add(BuildRedirectionAst(cmdElementTree));
                }
            }

            return redirectionAsts;
        }

        private IEnumerable<RedirectionAst> BuildRedirectionsAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.redirections);

            foreach (ParseTreeNode child in parseTreeNode.ChildNodes)
            {
                yield return BuildRedirectionAst(child);
            }
        }

        private RedirectionAst BuildRedirectionAst(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.redirection);

            if (parseTreeNode.ChildNodes[0].Term == this._grammar.file_redirection_operator)
            {
                return new FileRedirectionAst(
                    new ScriptExtent(parseTreeNode),
                    RedirectionStream.Output,
                    BuildCommandArgumentAst(parseTreeNode.ChildNodes[1].ChildNodes[0]),
                    IsFileRedirectionAppendOperator(parseTreeNode.ChildNodes[0]));
            }
            else
            {
                throw new NotImplementedException(parseTreeNode.ToString());
            }
        }

        private bool IsFileRedirectionAppendOperator(ParseTreeNode parseTreeNode)
        {
            VerifyTerm(parseTreeNode, this._grammar.file_redirection_operator);

            switch (parseTreeNode.Token.ValueString)
            {
                case ">>":
                    return true;
                case ">":
                    return false;
                default:
                    throw new NotImplementedException(parseTreeNode.ToString());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Language;
using Pash.Implementation;
using System.Management.Automation;
using System.Reflection;
using System.Management.Automation.Runspaces;
using System.Collections;

namespace System.Management.Pash.Implementation
{
    class ExecutionVisitor : AstVisitor
    {
        readonly ExecutionContext _context;
        readonly PipelineCommandRuntime _pipelineCommandRuntime;

        public ExecutionVisitor(ExecutionContext context, PipelineCommandRuntime pipelineCommandRuntime)
        {
            this._context = context;
            this._pipelineCommandRuntime = pipelineCommandRuntime;
        }

        ExecutionVisitor CloneSub()
        {
            return new ExecutionVisitor(
                this._context.CreateNestedContext(),
                new PipelineCommandRuntime(this._pipelineCommandRuntime.pipelineProcessor)
                );
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            this._pipelineCommandRuntime.WriteObject(EvaluateBinaryExpression(binaryExpressionAst), true);
            return AstVisitAction.SkipChildren;
        }

        object EvaluateBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            var leftOperand = EvaluateAst(binaryExpressionAst.Left);
            var rightOperand = EvaluateAst(binaryExpressionAst.Right);

            switch (binaryExpressionAst.Operator)
            {
                case TokenKind.DotDot:
                    return Range((int)leftOperand, (int)rightOperand);

                case TokenKind.Plus:
                    return Add(leftOperand, rightOperand);

                case TokenKind.Ieq:
                    if (leftOperand.GetType() == typeof(int)) return ((int)leftOperand) == ((int)rightOperand);
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Multiply:
                case TokenKind.Divide:
                case TokenKind.Minus:
                case TokenKind.Equals:
                case TokenKind.PlusEquals:
                case TokenKind.MinusEquals:
                case TokenKind.MultiplyEquals:
                case TokenKind.DivideEquals:
                case TokenKind.RemainderEquals:
                case TokenKind.Format:
                case TokenKind.Not:
                case TokenKind.Bnot:
                case TokenKind.And:
                case TokenKind.Or:
                case TokenKind.Xor:
                case TokenKind.Band:
                case TokenKind.Bor:
                case TokenKind.Bxor:
                case TokenKind.Join:
                case TokenKind.Ine:
                case TokenKind.Ige:
                case TokenKind.Igt:
                case TokenKind.Ilt:
                case TokenKind.Ile:
                case TokenKind.Ilike:
                case TokenKind.Inotlike:
                case TokenKind.Imatch:
                case TokenKind.Inotmatch:
                case TokenKind.Ireplace:
                case TokenKind.Icontains:
                case TokenKind.Inotcontains:
                case TokenKind.Iin:
                case TokenKind.Inotin:
                case TokenKind.Isplit:
                case TokenKind.Ceq:
                case TokenKind.Cne:
                case TokenKind.Cge:
                case TokenKind.Cgt:
                case TokenKind.Clt:
                case TokenKind.Cle:
                case TokenKind.Clike:
                case TokenKind.Cnotlike:
                case TokenKind.Cmatch:
                case TokenKind.Cnotmatch:
                case TokenKind.Creplace:
                case TokenKind.Ccontains:
                case TokenKind.Cnotcontains:
                case TokenKind.Cin:
                case TokenKind.Cnotin:
                case TokenKind.Csplit:
                case TokenKind.Is:
                case TokenKind.IsNot:
                case TokenKind.As:
                case TokenKind.Shl:
                case TokenKind.Shr:
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                default:
                    throw new InvalidOperationException(binaryExpressionAst.ToString());
            }
        }

        IEnumerable<int> Range(int start, int end)
        {
            //// Description:
            ////
            //// A range-expression creates an unconstrained 1-dimensional array whose elements are the values of 
            //// the int sequence specified by the range bounds. The values designated by the operands are converted 
            //// to int, if necessary (§6.4). 

            //// The operand designating the lower value after conversion is the lower 
            //// bound, while the operand designating the higher value after conversion is the upper bound. 
            if (start < end)
            {
                return Extensions.Enumerable._.Generate(start, i => i + 1, end);
            }

            //// Both bounds may be the same, in which case, the resulting array has length 1. 
            if (start == end) return new[] { start };

            //// If the left operand designates the lower bound, the sequence is in ascending order. If the left 
            //// operand designates the upper bound, the sequence is in descending order.
            if (end < start)
            {
                return Extensions.Enumerable._.Generate(start, i => i - 1, end);
            }

            //// [Note: Conceptually, this operator is a shortcut for the corresponding binary comma operator 
            //// sequence. For example, the range 5..8 can also be generated using 5,6,7,8. However, if an ascending 
            //// or descending sequence is needed without having an array, an implementation may avoid generating an 
            //// actual array. For example, in foreach ($i in 1..5) { … }, no array need be created. end note]
            ////
            //// A range-expression can be used to specify an array slice (§9.9).

            throw new Exception("unreachable");
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (commandAst.InvocationOperator == TokenKind.Dot) throw new NotImplementedException(commandAst.ToString());

            Pipeline pipeline = this._context.CurrentRunspace.CreateNestedPipeline();

            pipeline.Input.Write(this._context.inputStreamReader.ReadToEnd(), true);

            var command = GetCommand(commandAst);
            List<CommandParameter> commandParameters = new List<CommandParameter>();

            // the first CommandElements is the command itself. The rest are parameters/arguments
            foreach (var commandElement in commandAst.CommandElements.Skip(1))
            {
                var commandParameterAst = commandElement as CommandParameterAst;
                var stringConstantExpressionAst = commandElement as StringConstantExpressionAst;

                if (commandParameterAst != null)
                {
                    commandParameters.Add(new CommandParameter(commandParameterAst.ParameterName, commandParameterAst.Argument));
                }

                else if (stringConstantExpressionAst != null)
                {
                    commandParameters.Add(new CommandParameter(null, stringConstantExpressionAst.Value));
                }
            }

            commandParameters.ForEach(commandParameter => command.Parameters.Add(commandParameter));
            pipeline.Commands.Add(command);

            this._context.PushPipeline(pipeline);
            try
            {
                // TODO: develop a rational model for null/singleton/collection
                var result = pipeline.Invoke();
                if (result.Any())
                {
                    this._pipelineCommandRuntime.WriteObject(result, true);
                }
            }
            finally
            {
                this._context.PopPipeline();
            }

            return AstVisitAction.SkipChildren;
        }

        Command GetCommand(CommandAst commandAst)
        {
            return new Command(commandAst.GetCommandName());
        }

        public object Add(object leftValue, object rightValue)
        {
            if (leftValue is PSObject) leftValue = ((PSObject)leftValue).ImmediateBaseObject;
            if (rightValue is PSObject) rightValue = ((PSObject)rightValue).ImmediateBaseObject;

            ////  7.7.1 Addition
            ////      Description:
            ////      
            ////          The result of the addition operator + is the sum of the values designated by the two operands after the usual arithmetic conversions (§6.15) have been applied.
            ////      
            ////          This operator is left associative.
            ////      
            ////      Examples:
            ////      
            ////          12 + -10L               # long result 2
            ////          -10.300D + 12           # decimal result 1.700
            ////          10.6 + 12               # double result 22.6
            ////          12 + "0xabc"            # int result 2760

            if (leftValue == null && rightValue == null) return null;

            if (leftValue == null) return rightValue;
            if (rightValue == null) return leftValue;

            if (leftValue is string) return leftValue + rightValue.ToString();

            if (leftValue is int) return (int)leftValue + Convert.ToInt32(rightValue);

            throw new NotImplementedException(this.ToString());
        }

        object EvaluateAst(Ast expressionAst)
        {
            var subVisitor = this.CloneSub();
            expressionAst.Visit(subVisitor);
            var result = subVisitor._pipelineCommandRuntime.outputResults.Read().SingleOrDefault();

            if (result is PSObject) return ((PSObject)result).BaseObject;
            if (result is IEnumerable<PSObject>) return ((IEnumerable<PSObject>)result).Select(o => o.BaseObject);
            return result;
        }

        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            this._pipelineCommandRuntime.outputResults.Write(constantExpressionAst.Value);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
        {
            // TODO: rewrite this - it should expand the commands in the original pipe

            PipelineCommandRuntime subRuntime = null;

            foreach (var pipelineElement in pipelineAst.PipelineElements)
            {
                ExecutionContext subContext = this._context.CreateNestedContext();

                if (subRuntime == null)
                {
                    subContext.inputStreamReader = this._context.inputStreamReader;
                }
                else
                {
                    subContext.inputStreamReader = new PSObjectPipelineReader(subRuntime.outputResults);
                }

                subRuntime = new PipelineCommandRuntime(this._pipelineCommandRuntime.pipelineProcessor);
                subContext.inputStreamReader = subContext.inputStreamReader;

                pipelineElement.Visit(new ExecutionVisitor(subContext, subRuntime));
            }

            this._pipelineCommandRuntime.WriteObject(subRuntime.outputResults.Read(), true);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
        {
            Hashtable hashTable = new Hashtable();

            foreach (var pair in hashtableAst.KeyValuePairs)
            {
                hashTable.Add(EvaluateAst(pair.Item1), EvaluateAst(pair.Item2));
            }

            this._pipelineCommandRuntime.WriteObject(hashTable);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            var variable = GetVariable(variableExpressionAst);
            this._pipelineCommandRuntime.WriteObject(variable.Value);

            return AstVisitAction.SkipChildren;
        }

        private PSVariable GetVariable(VariableExpressionAst variableExpressionAst)
        {
            var variable = this._context.SessionState.SessionStateGlobal.GetVariable(variableExpressionAst.VariablePath.UserPath);

            return variable;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            var variableExpressionAst = assignmentStatementAst.Left as VariableExpressionAst;
            if (variableExpressionAst == null) throw new NotImplementedException(assignmentStatementAst.ToString());

            var variable = this._context.SessionState.SessionStateGlobal.SetVariable(variableExpressionAst.VariablePath.UserPath, EvaluateAst(assignmentStatementAst.Right));
            this._pipelineCommandRuntime.WriteObject(variable);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            var functionInfo = new /*FunctionInfo*/ScriptInfo(functionDefinitionAst.Name, functionDefinitionAst.Body.GetScriptBlock());

            // HACK: we shouldn't be casting this. But I'm too confused about runspace management in Pash.
            ((LocalRunspace)this._context.CurrentRunspace).CommandManager.SetFunction(functionInfo);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            var targetValue = EvaluateAst(indexExpressionAst.Target);

            int index = (int)EvaluateAst(indexExpressionAst.Index);

            var stringTargetValue = targetValue as string;
            if (stringTargetValue != null)
            {
                var result = stringTargetValue[index];
                this._pipelineCommandRuntime.WriteObject(result);
            }

            else throw new NotImplementedException(indexExpressionAst.ToString() + " " + targetValue.GetType());

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitIfStatement(IfStatementAst ifStatementAst)
        {
            ////    8.3 The if statement
            ////        The pipeline controlling expressions must have type bool or be implicitly convertible to that 
            ////        type. The else-clause is optional. There may be zero or more elseif-clauses.
            ////        
            ////        If the top-level pipeline tests True, then its statement-block is executed and execution of 
            ////        the statement terminates. Otherwise, if an elseif-clause is present, if its pipeline tests 
            ////        True, then its statement-block is executed and execution of the statement terminates. 
            ////        Otherwise, if an else-clause is present, its statement-block is executed.

            foreach (var clause in ifStatementAst.Clauses)
            {
                var condition = EvaluateAst(clause.Item1);

                if (condition == null) continue;

                if (condition.GetType() == typeof(bool))
                {
                    if ((bool)condition)
                    {
                        this._pipelineCommandRuntime.WriteObject(EvaluateAst(clause.Item2));
                        return AstVisitAction.SkipChildren;
                    }
                }

                else throw new NotImplementedException(ifStatementAst.ToString());
            }

            if (ifStatementAst.ElseClause != null)
            {
                // iterating over a statement list should be its own method.
                foreach (var statement in ifStatementAst.ElseClause.Statements)
                {
                    this._pipelineCommandRuntime.WriteObject(EvaluateAst(statement));
                }
            }

            return AstVisitAction.SkipChildren;
        }

        #region  NYI
        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            throw new NotImplementedException(); //VisitArrayExpression(arrayExpressionAst);
        }

        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            throw new NotImplementedException(); //VisitArrayLiteral(arrayLiteralAst);
        }

        public override AstVisitAction VisitAttribute(AttributeAst attributeAst)
        {
            throw new NotImplementedException(); //VisitAttribute(attributeAst);
        }

        public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            throw new NotImplementedException(); //VisitAttributedExpression(attributedExpressionAst);
        }

        public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            throw new NotImplementedException(); //VisitBreakStatement(breakStatementAst);
        }

        public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            throw new NotImplementedException(); //VisitCatchClause(catchClauseAst);
        }

        public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            // just iterate over children
            return base.VisitCommandExpression(commandExpressionAst);
        }

        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            throw new NotImplementedException(); //VisitCommandParameter(commandParameterAst);
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            throw new NotImplementedException(); //VisitContinueStatement(continueStatementAst);
        }

        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            throw new NotImplementedException(); //VisitConvertExpression(convertExpressionAst);
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
        {
            throw new NotImplementedException(); //VisitDataStatement(dataStatementAst);
        }

        public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            throw new NotImplementedException(); //VisitDoUntilStatement(doUntilStatementAst);
        }

        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            throw new NotImplementedException(); //VisitDoWhileStatement(doWhileStatementAst);
        }

        public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            throw new NotImplementedException(); //VisitExitStatement(exitStatementAst);
        }

        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            throw new NotImplementedException(); //VisitExpandableStringExpression(expandableStringExpressionAst);
        }

        public override AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst)
        {
            throw new NotImplementedException(); //VisitFileRedirection(redirectionAst);
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            throw new NotImplementedException(); //VisitForEachStatement(forEachStatementAst);
        }

        public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
        {
            throw new NotImplementedException(); //VisitForStatement(forStatementAst);
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            throw new NotImplementedException(); //VisitInvokeMemberExpression(methodCallAst);
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            throw new NotImplementedException(); //VisitMemberExpression(memberExpressionAst);
        }

        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst redirectionAst)
        {
            throw new NotImplementedException(); //VisitMergingRedirection(redirectionAst);
        }

        public override AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            throw new NotImplementedException(); //VisitNamedAttributeArgument(namedAttributeArgumentAst);
        }

        public override AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            // just iterate over children
            return base.VisitNamedBlock(namedBlockAst);
        }

        public override AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            throw new NotImplementedException(); //VisitParamBlock(paramBlockAst);
        }

        public override AstVisitAction VisitParameter(ParameterAst parameterAst)
        {
            throw new NotImplementedException(); //VisitParameter(parameterAst);
        }

        public override AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            // just iterate over children
            return base.VisitParenExpression(parenExpressionAst);
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            throw new NotImplementedException(); //VisitReturnStatement(returnStatementAst);
        }

        public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            // just iterate over children
            return base.VisitScriptBlock(scriptBlockAst);
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            throw new NotImplementedException(); //VisitScriptBlockExpression(scriptBlockExpressionAst);
        }

        public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            return base.VisitStatementBlock(statementBlockAst);
        }

        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            throw new NotImplementedException(); //VisitStringConstantExpression(stringConstantExpressionAst);
        }

        public override AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            throw new NotImplementedException(); //VisitSubExpression(subExpressionAst);
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            throw new NotImplementedException(); //VisitSwitchStatement(switchStatementAst);
        }

        public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            throw new NotImplementedException(); //VisitThrowStatement(throwStatementAst);
        }

        public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
        {
            throw new NotImplementedException(); //VisitTrap(trapStatementAst);
        }

        public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
        {
            throw new NotImplementedException(); //VisitTryStatement(tryStatementAst);
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            throw new NotImplementedException(); //VisitTypeConstraint(typeConstraintAst);
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            throw new NotImplementedException(); //VisitUnaryExpression(unaryExpressionAst);
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            throw new NotImplementedException(); //VisitUsingExpression(usingExpressionAst);
        }

        public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            throw new NotImplementedException(); //VisitWhileStatement(whileStatementAst);
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            throw new NotImplementedException(typeExpressionAst.ToString());
        }
        #endregion
    }
}

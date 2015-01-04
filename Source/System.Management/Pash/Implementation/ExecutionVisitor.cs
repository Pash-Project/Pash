// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Language;
using Pash;
using Pash.Implementation;
using System.Management.Automation;
using System.Reflection;
using System.Management.Automation.Runspaces;
using System.Collections;
using Extensions.Enumerable;
using System.Text.RegularExpressions;
using Pash.ParserIntrinsics;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;
using Extensions.Reflection;
using System.Security.Policy;
using System.Security.Cryptography;

namespace System.Management.Pash.Implementation
{
    class ExecutionVisitor : AstVisitor
    {
        readonly PipelineCommandRuntime _pipelineCommandRuntime;
        readonly bool _writeSideEffectsToPipeline;

        internal readonly ExecutionContext ExecutionContext;

        public ExecutionVisitor(ExecutionContext context, PipelineCommandRuntime pipelineCommandRuntime, bool writeSideEffectsToPipeline = false)
        {
            this.ExecutionContext = context;
            this._pipelineCommandRuntime = pipelineCommandRuntime;
            this._writeSideEffectsToPipeline = writeSideEffectsToPipeline;
        }

        ExecutionVisitor CloneSub(bool writeSideEffectsToPipeline)
        {
            var subContext = this.ExecutionContext.CreateNestedContext();
            var subRuntime = new PipelineCommandRuntime(this._pipelineCommandRuntime.PipelineProcessor);
            subRuntime.ExecutionContext = subContext;
            return new ExecutionVisitor(
                subContext,
                subRuntime,
                writeSideEffectsToPipeline
                );
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            // Important: never enumerate when writing to pipeline
            // Array replication is a binary expression. Replicated arrays might be huge, writing all
            // elements to pipeline can cause crucial performance issues if the array is needed again
            this._pipelineCommandRuntime.WriteObject(EvaluateBinaryExpression(binaryExpressionAst), false);
            return AstVisitAction.SkipChildren;
        }

        object EvaluateBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            var leftOperand = EvaluateAst(binaryExpressionAst.Left);
            var rightOperand = EvaluateAst(binaryExpressionAst.Right);

            if (leftOperand is PSObject) leftOperand = ((PSObject)leftOperand).BaseObject;
            if (rightOperand is PSObject) rightOperand = ((PSObject)rightOperand).BaseObject;

            int? leftOperandInt = leftOperand is int ? ((int?)leftOperand) : null;
            int? rightOperandInt = rightOperand is int ? ((int?)rightOperand) : null;

            switch (binaryExpressionAst.Operator)
            {
                case TokenKind.DotDot:
                    return Range(LanguagePrimitives.ConvertTo<int>(leftOperand), LanguagePrimitives.ConvertTo<int>(rightOperand));

                case TokenKind.Plus:
                    return ArithmeticOperations.Add(leftOperand, rightOperand);

                case TokenKind.Ieq:
                case TokenKind.Ceq:
                    if (leftOperand is string || rightOperand is string)
                    {
                        StringComparison ignoreCaseComparision =
                            (TokenKind.Ceq == binaryExpressionAst.Operator)
                            ? StringComparison.CurrentCulture
                            : StringComparison.CurrentCultureIgnoreCase;
                        var left = LanguagePrimitives.ConvertTo<string>(leftOperand);
                        var right = LanguagePrimitives.ConvertTo<string>(rightOperand);
                        return String.Equals(left, right, ignoreCaseComparision);
                    }
                    return Object.Equals(leftOperand, rightOperand);

                case TokenKind.Ine:
                case TokenKind.Cne:
                    if (leftOperand is string || rightOperand is string)
                    {
                        StringComparison ignoreCaseComparision =
                            (TokenKind.Cne == binaryExpressionAst.Operator)
                            ? StringComparison.CurrentCulture
                            : StringComparison.CurrentCultureIgnoreCase;
                        var left = LanguagePrimitives.ConvertTo<string>(leftOperand);
                        var right = LanguagePrimitives.ConvertTo<string>(rightOperand);
                        return !String.Equals(left, right, ignoreCaseComparision);
                    }
                    return !Object.Equals(leftOperand, rightOperand);

                case TokenKind.Igt:
                    if (leftOperandInt.HasValue) return leftOperandInt > rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Ige:
                    if (leftOperandInt.HasValue) return leftOperandInt >= rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Or:
                    return LanguagePrimitives.ConvertTo<bool>(leftOperand) || LanguagePrimitives.ConvertTo<bool>(rightOperand);

                case TokenKind.Xor:
                    return LanguagePrimitives.ConvertTo<bool>(leftOperand) != LanguagePrimitives.ConvertTo<bool>(rightOperand);

                case TokenKind.And:
                    return LanguagePrimitives.ConvertTo<bool>(leftOperand) && LanguagePrimitives.ConvertTo<bool>(rightOperand);

                case TokenKind.Ilt:
                    if (leftOperandInt.HasValue) return leftOperandInt < rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Ile:
                    if (leftOperandInt.HasValue) return leftOperandInt <= rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Band:
                    return BitwiseOperation.And(leftOperand, rightOperand);
                case TokenKind.Bor:
                    return BitwiseOperation.Or(leftOperand, rightOperand);
                case TokenKind.Bxor:
                    return BitwiseOperation.Xor(leftOperand, rightOperand);

                case TokenKind.Imatch:
                    return Match(leftOperand, rightOperand, RegexOptions.IgnoreCase);
                case TokenKind.Inotmatch:
                    return NotMatch(leftOperand, rightOperand, RegexOptions.IgnoreCase);
                case TokenKind.Cmatch:
                    return Match(leftOperand, rightOperand, RegexOptions.None);
                case TokenKind.Cnotmatch:
                    return NotMatch(leftOperand, rightOperand, RegexOptions.None);

                case TokenKind.Multiply:
                    return ArithmeticOperations.Multiply(leftOperand, rightOperand);

                case TokenKind.Divide:
                    return ArithmeticOperations.Divide(leftOperand, rightOperand);

                case TokenKind.Minus:
                    return ArithmeticOperations.Subtract(leftOperand, rightOperand);

                case TokenKind.Rem:
                    return ArithmeticOperations.Remainder(leftOperand, rightOperand);

                case TokenKind.Format:
                    {
                        var left = LanguagePrimitives.ConvertTo<string>(leftOperand);
                        var right = LanguagePrimitives.ConvertTo<object[]>(rightOperand);
                        return string.Format(left, right);
                    }

                case TokenKind.Equals:
                case TokenKind.PlusEquals:
                case TokenKind.MinusEquals:
                case TokenKind.MultiplyEquals:
                case TokenKind.DivideEquals:
                case TokenKind.RemainderEquals:
                case TokenKind.Not:
                case TokenKind.Bnot:
                case TokenKind.Join:
                case TokenKind.Ilike:
                case TokenKind.Inotlike:
                case TokenKind.Ireplace:
                case TokenKind.Icontains:
                case TokenKind.Inotcontains:
                case TokenKind.Iin:
                case TokenKind.Inotin:
                case TokenKind.Isplit:
                case TokenKind.Cge:
                case TokenKind.Cgt:
                case TokenKind.Clt:
                case TokenKind.Cle:
                case TokenKind.Clike:
                case TokenKind.Cnotlike:
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

        private bool Match(object leftOperand, object rightOperand, RegexOptions regexOptions)
        {
            if (!(leftOperand is string) || !(rightOperand is string))
                throw new NotImplementedException(string.Format("{0} -match {1}", leftOperand, rightOperand));

            Regex regex = new Regex((string)rightOperand, regexOptions);
            Match match = regex.Match((string)leftOperand);

            SetMatchesVariable(regex, match);

            return match.Success;
        }

        private bool NotMatch(object leftOperand, object rightOperand, RegexOptions regexOptions)
        {
            if (!(leftOperand is string) || !(rightOperand is string))
                throw new NotImplementedException(string.Format("{0} -match {1}", leftOperand, rightOperand));

            Regex regex = new Regex((string)rightOperand, regexOptions);
            Match match = regex.Match((string)leftOperand);

            return !match.Success;
        }

        private void SetMatchesVariable(Regex regex, Match match)
        {
            var matches = new Hashtable();
            var groupNames = from name in regex.GetGroupNames()
                             where match.Groups[name].Success
                             select name;

            foreach (string name in groupNames)
            {
                int num;
                if (int.TryParse(name, out num))
                {
                    matches.Add(num, match.Groups[num].Value);
                }
                else
                {
                    matches.Add(name, match.Groups[name].Value);
                }
            }

            ExecutionContext.SetVariable("Matches", PSObject.AsPSObject(matches));
        }

        private IEnumerable<int> Range(int start, int end)
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
                return Extensions.Enumerable._.Generate(start, i => i + 1, end).ToArray();
            }

            //// Both bounds may be the same, in which case, the resulting array has length 1. 
            if (start == end) return new[] { start };

            //// If the left operand designates the lower bound, the sequence is in ascending order. If the left 
            //// operand designates the upper bound, the sequence is in descending order.
            if (end < start)
            {
                return Extensions.Enumerable._.Generate(start, i => i - 1, end).ToArray();
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
            throw new Exception("Unreachable, should be part of a pipeline. Please report this!");
        }

        CommandParameter ConvertCommandElementToCommandParameter(CommandElementAst commandElement)
        {
            if (commandElement is CommandParameterAst)
            {
                var commandParameterAst = commandElement as CommandParameterAst;
                object arg = commandParameterAst.Argument == null ? null : EvaluateAst(commandParameterAst.Argument);
                return new CommandParameter(commandParameterAst.ParameterName, arg,
                                            commandParameterAst.RequiresArgument);
            }

            else if (commandElement is StringConstantExpressionAst)
            {
                var stringConstantExpressionAst = commandElement as StringConstantExpressionAst;
                return new CommandParameter(null, stringConstantExpressionAst.Value);
            }

            else if (commandElement is ExpressionAst)
            {
                return new CommandParameter(null, EvaluateAst(commandElement));
            }

            else throw new NotImplementedException();
        }

        Command GetCommand(CommandAst commandAst)
        {
            var firstCommandElement = commandAst.CommandElements.First();
            object command = null;
            bool useLocalScope = commandAst.InvocationOperator != TokenKind.Dot;
            if (firstCommandElement is ScriptBlockExpressionAst)
            {
                command = (firstCommandElement as ScriptBlockExpressionAst).ScriptBlock;
            }
            else //otherwise we evaluate it and get the result
            {
                command = EvaluateAst(firstCommandElement);
                if (command is PSObject)
                {
                    command = (command as PSObject).BaseObject;
                }
            }
            //if it's a script block, we are only interested in its Ast (which is indeed always a ScriptBlockAst)
            if (command is ScriptBlock)
            {
                command = (command as ScriptBlock).Ast as ScriptBlockAst;
            }
            //let's check if we got something useful to execute
            if (command is ScriptBlockAst)
            {
                return new Command(command as ScriptBlockAst, useLocalScope);
            }
            else //all other objects will converted as a string with ToString(). This is normal powershell behavior!
            {
                return new Command(command.ToString(), false, useLocalScope);
            }
        }

        public object EvaluateAst(Ast expressionAst)
        {
            return EvaluateAst(expressionAst, true);
        }

        public object EvaluateAst(Ast expressionAst, bool writeSideEffectsToPipeline)
        {
            var subVisitor = this.CloneSub(writeSideEffectsToPipeline);
            expressionAst.Visit(subVisitor);
            var result = subVisitor._pipelineCommandRuntime.OutputStream.Read();

            if (result.Count == 0)
            {
                return null;
            }
            else if (result.Count == 1)
            {
                return result.Single();
            }
            return result.ToArray();
        }

        public bool EvaluateLoopBodyAst(Ast expressionAst, string loopLabel)
        {
            var loopCanContinue = true;

            try
            {
                expressionAst.Visit(this);
            }
            catch (LoopFlowException e)
            {
                if (!String.IsNullOrEmpty(e.Label) && !e.Label.Equals(loopLabel))
                {
                    throw;
                }
                loopCanContinue = e is ContinueException;
            }

            // return if the loop can continue execution or should break
            return loopCanContinue;
        }

        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            this._pipelineCommandRuntime.OutputStream.Write(constantExpressionAst.Value);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
        {
            // shouldn't happen, but anyway
            if (!pipelineAst.PipelineElements.Any())
            {
                return AstVisitAction.SkipChildren;
            }
            // Pipeline uses global execution context, so we should set its WriteSideEffects flag, and restore it after.
            // TODO: I'm not very sure about that changing context and WriteSideEffectsToPipeline stuff
            var pipeLineContext = ExecutionContext.CurrentRunspace.ExecutionContext;
            bool writeSideEffects = pipeLineContext.WriteSideEffectsToPipeline;
            try
            {
                pipeLineContext.WriteSideEffectsToPipeline = _writeSideEffectsToPipeline;
                var pipeline = ExecutionContext.CurrentRunspace.CreateNestedPipeline();
                int startAt = 0; // set to 1 if first element is an expression
                int pipelineCommandCount = pipelineAst.PipelineElements.Count;

                // first element of pipeline can be an expression that needs to be evaluated
                var expression = pipelineAst.PipelineElements[0] as CommandExpressionAst;
                if (expression != null)
                {
                    // evaluate it and get results
                    var subVisitor = this.CloneSub(_writeSideEffectsToPipeline);
                    expression.Expression.Visit(subVisitor);
                    var results = subVisitor._pipelineCommandRuntime.OutputStream.Read();
                    // if we only have that one expression and no commands, write expression to output and return
                    if (pipelineCommandCount == 1)
                    {
                        _pipelineCommandRuntime.WriteObject(results, true);
                        VisitRedirections(expression);
                        return AstVisitAction.SkipChildren;
                    }
                    // otherwise write value to input of pipeline to be processed
                    if (results.Count == 1)
                    {
                        // "unroll" an input array: write all enumerated elements seperately to the pipeline
                        pipeline.Input.Write(results[0], true);
                    }
                    else
                    {
                        pipeline.Input.Write(results, true);
                    }
                    startAt = 1;
                }
                else // if there was no expression we take the input of the context's input stream
                {
                    foreach (var input in ExecutionContext.InputStream.Read())
                    {
                        pipeline.Input.Write(input);
                    }
                }

                // all other elements *need* to be commands (same in PS). Make that sure and add them to the pipeline
                for (int curCommand = startAt; curCommand < pipelineCommandCount; curCommand++)
                {
                    var commandAst = pipelineAst.PipelineElements[curCommand] as CommandAst;
                    if (commandAst == null)
                    {
                        throw new NotSupportedException("Invalid command in pipeline."
                            + " Only the first element of a pipeline can be an expression.");
                    }
                    var command = GetCommand(commandAst);

                    commandAst.CommandElements
                    // the first CommandElements is the command itself. The rest are parameters/arguments
                    .Skip(1)
                        .Select(ConvertCommandElementToCommandParameter)
                        .ForEach(command.Parameters.Add);

                    command.RedirectionVisitor = new RedirectionVisitor(this, commandAst.Redirections);

                    pipeline.Commands.Add(command);
                }

                // now execute the pipeline
                ExecutionContext.PushPipeline(pipeline);
                // rerouting the output and error stream would be easier, but Pipeline doesn't
                // have an interface for this. So let's catch the exception for now, read the streams
                // and rethrow it afterwards
                Exception exception = null;
                Collection<PSObject> pipeResults = null;
                try
                {
                    pipeResults = pipeline.Invoke();
                }
                catch (Exception e)
                {
                    exception = e;
                    pipeResults = pipeline.Output.NonBlockingRead();
                }
                // read output and error and write them as results of the current commandRuntime
                foreach (var curResult in pipeResults)
                {
                    _pipelineCommandRuntime.WriteObject(curResult);
                }
                var errors = pipeline.Error.NonBlockingRead();
                foreach (var curError in errors)
                {
                    _pipelineCommandRuntime.ErrorStream.Write(curError);
                }
                ExecutionContext.PopPipeline();
                if (exception != null)
                {
                    throw exception;
                }
            }
            finally
            {
                pipeLineContext.WriteSideEffectsToPipeline = writeSideEffects;
            }

            return AstVisitAction.SkipChildren;
        }

        private void VisitRedirections(CommandBaseAst commandAst)
        {
            if (commandAst.Redirections.Count == 0)
            {
                return;
            }

            foreach (RedirectionAst redirectAst in commandAst.Redirections)
            {
                var fileRedirectAst = redirectAst as FileRedirectionAst;
                if (fileRedirectAst != null)
                {
                    VisitFileRedirection(fileRedirectAst);
                }
                else
                {
                    throw new NotImplementedException(redirectAst.ToString());
                }
            }
        }

        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
        {
            Hashtable hashTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            foreach (var pair in hashtableAst.KeyValuePairs)
            {
                // if we don't have a custom psobject, make sure the value
                var val = EvaluateAst(pair.Item2);
                var psobjVal = val as PSObject;
                if (psobjVal != null && psobjVal.ImmediateBaseObject != null &&
                    psobjVal.ImmediateBaseObject.GetType() != typeof(PSCustomObject))
                {
                    val = psobjVal.ImmediateBaseObject;
                }
                hashTable.Add(EvaluateAst(pair.Item1), val);
            }

            this._pipelineCommandRuntime.WriteObject(hashTable);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            _pipelineCommandRuntime.WriteObject(new SettableVariableExpression(variableExpressionAst, this).GetValue());
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            // first check if it's a assignable expression, fail otherwise. later on we also need support for 
            // arrays of variables
            ExpressionAst expressionAst = assignmentStatementAst.Left;
            if (!SettableExpression.SupportedExpressions.Contains(expressionAst.GetType()))
            {
                var msg = String.Format("The expression type '{0}' is currently not supported for assignments",
                                        expressionAst.GetType().ToString());
                throw new NotImplementedException(msg);
            }
            var assignableExpression = SettableExpression.Create(expressionAst, this);
            var rightValue = EvaluateAst(assignmentStatementAst.Right);
            object newValue = rightValue;

            bool isSimpleAssignment = assignmentStatementAst.Operator == TokenKind.Equals;
            // safe the effort to get the value of the left side if it's replaced anyway
            dynamic currentValue = isSimpleAssignment ? null : assignableExpression.GetValue();


            // compute the new value
            if (isSimpleAssignment)
            {
                newValue = rightValue;
            }
            else if (assignmentStatementAst.Operator == TokenKind.PlusEquals)
            {
                newValue = ArithmeticOperations.Add(currentValue, rightValue);
            }
            else if (assignmentStatementAst.Operator == TokenKind.MinusEquals)
            {
                newValue = ArithmeticOperations.Subtract(currentValue, rightValue);
            }
            else if (assignmentStatementAst.Operator == TokenKind.MultiplyEquals)
            {
                newValue = ArithmeticOperations.Multiply(currentValue, rightValue);
            }
            else if (assignmentStatementAst.Operator == TokenKind.DivideEquals)
            {
                newValue = ArithmeticOperations.Divide(currentValue, rightValue);
            }
            else if (assignmentStatementAst.Operator == TokenKind.RemainderEquals)
            {
                newValue = ArithmeticOperations.Remainder(currentValue, rightValue);
            }
            else
            {
                throw new NotImplementedException("Unsupported operator: " + assignmentStatementAst.Operator.ToString());
            }

            if (this._writeSideEffectsToPipeline)
            {
                this._pipelineCommandRuntime.WriteObject(newValue);
            }

            // set the new value
            assignableExpression.SetValue(newValue);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            ExecutionContext.SessionState.Function.Set(functionDefinitionAst.Name, functionDefinitionAst.Body.GetScriptBlock(),
                                               functionDefinitionAst.Parameters, "");
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            _pipelineCommandRuntime.WriteObject(new SettableIndexExpression(indexExpressionAst, this).GetValue(), false);
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

                // null is false
                if (condition == null) continue;

                else if (condition is IList && ((IList)condition).Count == 0) continue;

                else if (condition is PSObject)
                {
                    var baseObject = ((PSObject)condition).BaseObject;

                    if (baseObject is bool && ((bool)baseObject) == false) continue;
                }

                else throw new NotImplementedException(clause.Item1.ToString());

                clause.Item2.Visit(this);
                return AstVisitAction.SkipChildren;
            }

            if (ifStatementAst.ElseClause != null)
            {
                // iterating over a statement list should be its own method.
                foreach (var statement in ifStatementAst.ElseClause.Statements)
                {
                    statement.Visit(this);
                }
            }

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            var psobj = PSObject.WrapOrNull(EvaluateAst(methodCallAst.Expression, false));
            if (psobj == null)
            {
                throw new PSInvalidOperationException("Cannot invoke a method of a NULL expression");
            }
            var memberNameObj = EvaluateAst(methodCallAst.Member, false);
            var method = PSObject.GetMemberInfoSafe(psobj, memberNameObj, methodCallAst.Static) as PSMethodInfo;
            if (method == null)
            {
                var msg = String.Format("The object has no method called '{0}'", memberNameObj.ToString());
                throw new PSArgumentException(msg);
            }
            var arguments = methodCallAst.Arguments.Select(EvaluateAst).Select(o => o is PSObject ? ((PSObject)o).BaseObject : o);
            var result = method.Invoke(arguments.ToArray());
            if (result != null)
            {
                _pipelineCommandRuntime.WriteObject(PSObject.AsPSObject(result));
            }
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            _pipelineCommandRuntime.WriteObject(new SettableMemberExpression(memberExpressionAst, this).GetValue());
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            var arrayList = new List<object>();
            foreach (var el in arrayLiteralAst.Elements)
            {
                arrayList.Add(EvaluateAst(el));
            }
            _pipelineCommandRuntime.WriteObject(arrayList.ToArray(), false);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            switch (unaryExpressionAst.TokenKind)
            {
                case TokenKind.PostfixPlusPlus:
                case TokenKind.PlusPlus:
                case TokenKind.MinusMinus:
                case TokenKind.PostfixMinusMinus:
                    VisitIncrementDecrementExpression(unaryExpressionAst);
                break;

                case TokenKind.Not:
                    var value = EvaluateAst(unaryExpressionAst.Child, _writeSideEffectsToPipeline);
                    var boolValue = (bool) LanguagePrimitives.ConvertTo(value, typeof(bool));
                    _pipelineCommandRuntime.WriteObject(!boolValue);
                    break;

                default:
                    throw new NotImplementedException(unaryExpressionAst.ToString());
            }

            return AstVisitAction.SkipChildren;
        }

        private void VisitIncrementDecrementExpression(UnaryExpressionAst unaryExpressionAst)
        {
            var token = unaryExpressionAst.TokenKind;

            // first validate the expression. Shouldn't fail, but let's be sure
            var validTokens = new [] { TokenKind.PostfixPlusPlus, TokenKind.PostfixMinusMinus,
                TokenKind.PlusPlus, TokenKind.MinusMinus};
            if (!validTokens.Contains(token))
            {
                throw new PSInvalidOperationException("The unary expression is not a decrement/increment expression. " +
                                                      "Please report this issue.");
            }

            bool postfix = token == TokenKind.PostfixPlusPlus || token == TokenKind.PostfixMinusMinus;
            bool increment = token == TokenKind.PostfixPlusPlus || token == TokenKind.PlusPlus;

            // It's the duty of the AstBuilderto check wether the child expression is a settable expression
            SettableExpression affectedExpression = SettableExpression.Create(unaryExpressionAst.Child, this);
            object objValue = PSObject.Unwrap(affectedExpression.GetValue());
            objValue = objValue ?? 0; // if the value is null, then we "convert" to integer 0, says the specification

            // check for non-numerics
            var valueType = objValue.GetType();
            if (!valueType.IsNumeric())
            {
                var msg = String.Format("The operator '{0}' can only be used for numbers. The operand is '{1}'",
                                        increment ? "++" : "--", valueType.FullName);
                throw new RuntimeException(msg, "OperatorRequiresNumber", ErrorCategory.InvalidOperation, objValue);
            }

            // if it's a postfix operation, then we need to write the numeric value to pipeline
            if (postfix && _writeSideEffectsToPipeline)
            {
                _pipelineCommandRuntime.WriteObject(objValue);
            }

            // now actually change the value, check for overflow
            dynamic dynValue = (dynamic)objValue;
            try
            {
                dynValue = checked(dynValue + (increment ? 1 : -1));
            }
            catch (OverflowException) // cast to double on overflow
            {
                dynValue = LanguagePrimitives.ConvertTo<double>(objValue) + (increment ? 1 : -1);
            }

            // set the new value
            affectedExpression.SetValue((object)dynValue);

            // if it was a prefix, then we need to write the new value to pipeline
            if (!postfix && _writeSideEffectsToPipeline)
            {
                _pipelineCommandRuntime.WriteObject(dynValue);
            }
        }

        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            List<object> elements = new List<object>();
            var numStatements = arrayExpressionAst.SubExpression.Statements.Count;
            foreach (var stmt in arrayExpressionAst.SubExpression.Statements)
            {
                var result = EvaluateAst(stmt, false);
                // expand if only one element and it is enumerable
                var enumerator = LanguagePrimitives.GetEnumerator(result);
                if (numStatements > 1 || enumerator == null)
                {
                    elements.Add(result);
                }
                else
                {
                    while (enumerator.MoveNext())
                    {
                        elements.Add(enumerator.Current);
                    }
                }
            }

            this._pipelineCommandRuntime.WriteObject(elements.ToArray());

            return AstVisitAction.SkipChildren;
        }

        #region  NYI
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
            var label = breakStatementAst.Label == null ? null : EvaluateAst(breakStatementAst.Label, false);
            throw new BreakException(LanguagePrimitives.ConvertTo<string>(label));
        }

        public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            throw new NotImplementedException(); //VisitCatchClause(catchClauseAst);
        }

        public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            throw new Exception("Unreachable, should be part of a pipeline. Please report this!");
        }

        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            throw new NotImplementedException(); //VisitCommandParameter(commandParameterAst);
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            var label = continueStatementAst.Label == null ? null : EvaluateAst(continueStatementAst.Label, false);
            throw new ContinueException(LanguagePrimitives.ConvertTo<string>(label));
        }

        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            Type type = convertExpressionAst.Type.TypeName.GetReflectionType();

            var value = EvaluateAst(convertExpressionAst.Child);

            _pipelineCommandRuntime.WriteObject(LanguagePrimitives.ConvertTo(value, type));
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
        {
            throw new NotImplementedException(); //VisitDataStatement(dataStatementAst);
        }

        public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            return VisitSimpleLoopStatement(doUntilStatementAst.Body, doUntilStatementAst.Condition, true, true);
        }

        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            return VisitSimpleLoopStatement(doWhileStatementAst.Body, doWhileStatementAst.Condition, true, false);
        }

        public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            /* The controlling expression while-condition must have type bool or
             * be implicitly convertible to that type. The loop body, which
             * consists of statement-block, is executed repeatedly while the
             * controlling expression tests True. The controlling expression
             * is evaluated before each execution of the loop body.
             */
            return VisitSimpleLoopStatement(whileStatementAst.Body, whileStatementAst.Condition, false, false);
        }

        private AstVisitAction VisitSimpleLoopStatement(StatementBlockAst body, PipelineBaseAst condition,
                                                    bool preExecuteBody, bool invertCond)
        {
            // TODO: pass loop label
            // preExecuteBody is for do while/until loops
            if (preExecuteBody && !EvaluateLoopBodyAst(body, null))
            {
                return AstVisitAction.SkipChildren;
            }

            // the condition is XORed with invertCond and menas: (true && !invertCond) || (false && invertCond)
            while (LanguagePrimitives.ConvertTo<bool>(EvaluateAst(condition)) ^ invertCond)
            {
                if (!EvaluateLoopBodyAst(body, null))
                {
                    break;
                }
            }
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            object arg = exitStatementAst.Pipeline == null ? null : EvaluateAst(exitStatementAst.Pipeline, false);
            throw new ExitException(arg);
        }

        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            var evaluatedExpressions = from expressionAst in expandableStringExpressionAst.NestedExpressions
                                       select EvaluateAst(expressionAst);

            string expandedString = expandableStringExpressionAst.ExpandString(evaluatedExpressions);
            this._pipelineCommandRuntime.OutputStream.Write(expandedString);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst)
        {
            return new FileRedirectionVisitor(this, _pipelineCommandRuntime).Visit(redirectionAst);
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            object enumerable = EvaluateAst(forEachStatementAst.Condition);
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(enumerable);

            if (enumerator == null)
            {
                enumerator = new [] { enumerable }.GetEnumerator();
            }

            while (enumerator.MoveNext())
            {
                this.ExecutionContext.SessionState.PSVariable.Set(forEachStatementAst.Variable.VariablePath.UserPath,
                                                          enumerator.Current);
                // TODO: pass the loop label
                if (!EvaluateLoopBodyAst(forEachStatementAst.Body, null))
                {
                    break;
                }
            }

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
        {
            /*
             * The controlling expression for-condition must have type bool or 
             * be implicitly convertible to that type. The loop body, which 
             * consists of statement-block, is executed repeatedly while the 
             * controlling expression tests True. The controlling expression 
             * is evaluated before each execution of the loop body.
             * 
             * Expression for-initializer is evaluated before the first 
             * evaluation of the controlling expression. Expression 
             * for-initializer is evaluated for its side effects only; any 
             * value it produces is discarded and is not written to the 
             * pipeline.
             * 
             * Expression for-iterator is evaluated after each execution of 
             * the loop body. Expression for-iterator is evaluated for its 
             * side effects only; any value it produces is discarded and is 
             * not written to the pipeline.
             * 
             * If expression for-condition is omitted, the controlling 
             * expression tests True.
             */

            if (forStatementAst.Initializer != null)
            {
                EvaluateAst(forStatementAst.Initializer);
            }

            while (forStatementAst.Condition != null ?
                      LanguagePrimitives.ConvertTo<bool>(EvaluateAst(forStatementAst.Condition))
                    : true)
            {
                // TODO: pass loop label
                if (!EvaluateLoopBodyAst(forStatementAst.Body, null))
                {
                    break;
                }
                if (forStatementAst.Iterator != null)
                {
                    EvaluateAst(forStatementAst.Iterator);
                }
            }

            return AstVisitAction.SkipChildren;
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
            if (namedBlockAst.Traps.Any())
            {
                return VisitNamedBlockWithTraps(namedBlockAst);
            }
            foreach (var stmt in namedBlockAst.Statements)
            {
                VisitStatement(stmt);
            }
            return AstVisitAction.SkipChildren;
        }

        private AstVisitAction VisitNamedBlockWithTraps(NamedBlockAst namedBlockAst)
        {
            foreach (StatementAst statement in namedBlockAst.Statements)
            {
                try
                {
                    VisitStatement(statement);
                }
                catch (FlowControlException)
                {
                    // flow control as Exit, Return, Break, and Continue doesn't concern the user and is propagated
                    throw;
                }
                catch (Exception ex)
                {
                    TrapStatementAst trapStatementAst = FindMatchingTrapStatement(namedBlockAst.Traps, ex);
                    if (trapStatementAst == null)
                    {
                        throw;
                    }

                    SetUnderscoreVariable(ex);
                    AstVisitAction visitAction = VisitTrapBody(trapStatementAst);

                    if (visitAction != AstVisitAction.Continue)
                    {
                        return AstVisitAction.SkipChildren;
                    }
                }
            }

            return AstVisitAction.SkipChildren;
        }

        private TrapStatementAst FindMatchingTrapStatement(ReadOnlyCollection<TrapStatementAst> trapStatements, Exception ex)
        {
            TrapStatementAst trapStatementAst = (from statement in trapStatements
                                                 where IsExactMatch(statement.TrapType, ex)
                                                 select statement).FirstOrDefault();
            if (trapStatementAst != null)
            {
                return trapStatementAst;
            }

            trapStatementAst = (from statement in trapStatements
                                where IsInheritedMatch(statement.TrapType, ex)
                                select statement).FirstOrDefault();

            if (trapStatementAst != null)
            {
                return trapStatementAst;
            }

            return (from statement in trapStatements
                    where statement.TrapType == null
                    select statement).FirstOrDefault();
        }

        private bool IsExactMatch(TypeConstraintAst typeConstraintAst, Exception ex)
        {
            return (typeConstraintAst != null) && (ex.GetType() == typeConstraintAst.TypeName.GetReflectionType());
        }

        private bool IsInheritedMatch(TypeConstraintAst typeConstraintAst, Exception ex)
        {
            return (typeConstraintAst != null) && (typeConstraintAst.TypeName.GetReflectionType().IsInstanceOfType(ex));
        }

        private AstVisitAction VisitTrapBody(TrapStatementAst trapStatement)
        {
            foreach (StatementAst statement in trapStatement.Body.Statements)
            {
                try
                {
                    statement.Visit(this);
                }
                catch (ContinueException)
                {
                    return AstVisitAction.Continue;
                }
                catch (BreakException)
                {
                    WriteErrorRecord();
                    return AstVisitAction.SkipChildren;
                }
            }

            WriteErrorRecord();
            return AstVisitAction.Continue;
        }

        private void WriteErrorRecord()
        {
            object error = ExecutionContext.GetVariableValue("_");
            _pipelineCommandRuntime.WriteObject(error);
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
            var value = EvaluateAst(parenExpressionAst.Pipeline);
            this._pipelineCommandRuntime.WriteObject(value);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            object value = null;
            if (returnStatementAst.Pipeline != null)
            {
                value = EvaluateAst(returnStatementAst.Pipeline, false);
            }

            throw new ReturnException(value);
        }

        public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            try
            {
                scriptBlockAst.EndBlock.Visit(this);
            }
            catch (ReturnException e)
            {
                // if the exception was a return excpetion, also write the returnResult, if existing
                if (PSObject.Unwrap(e.Value) != null)
                {
                    _pipelineCommandRuntime.WriteObject(e.Value, true);
                }
            }

            return AstVisitAction.SkipChildren;
        }

        private AstVisitAction VisitStatement(StatementAst statementAst)
        {
            // We execute this in a subcontext, because single enumerable results are 
            // evaluated and its values are written to pipeline (i.e. ". {1,2,3}" writes
            // all three values to pipeline
            // therefore we have to catch exceptions for now to read and write
            // the subVisitors' output stream, so that already written results appear
            // also in this OutputStream
            var subVisitor = this.CloneSub(false);
            subVisitor._pipelineCommandRuntime.ErrorStream.Redirect(_pipelineCommandRuntime.ErrorStream);
            try
            {
                statementAst.Visit(subVisitor);
            }
            finally
            {
                var result = subVisitor._pipelineCommandRuntime.OutputStream.Read();
                if (result.Count == 1)
                {
                    // this is the actual thing of this whole work: enumerate a single value!
                    _pipelineCommandRuntime.WriteObject(result.Single(), true);
                }
                else if (result.Count > 1)
                {
                    _pipelineCommandRuntime.WriteObject(result, true);
                }
            }
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            this._pipelineCommandRuntime.WriteObject(new ScriptBlock(scriptBlockExpressionAst.ScriptBlock));

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            return base.VisitStatementBlock(statementBlockAst);
        }

        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            this._pipelineCommandRuntime.OutputStream.Write(stringConstantExpressionAst.Value);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            object[] results = (from statementAst in subExpressionAst.SubExpression.Statements
                                let result = EvaluateAst(statementAst, false)
                                where result != null
                                select result).ToArray();

            if (results.Length == 1)
            {
                _pipelineCommandRuntime.WriteObject(results.Single());
            }
            else if (results.Length > 0)
            {
                _pipelineCommandRuntime.WriteObject(results);
            }
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            throw new NotImplementedException(); //VisitSwitchStatement(switchStatementAst);
        }

        public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            object targetObject = GetTargetObject(throwStatementAst);
            if (targetObject is Exception)
            {
                throw (Exception)targetObject;
            }

            string errorMessage = GetErrorMessageForThrowStatement(targetObject);
            throw new RuntimeException(errorMessage);
        }

        private object GetTargetObject(ThrowStatementAst throwStatementAst)
        {
            if (throwStatementAst.Pipeline != null)
            {
                object targetObject = EvaluateAst(throwStatementAst.Pipeline, false);
                if (targetObject is PSObject)
                {
                    return ((PSObject)targetObject).BaseObject;
                }
                return targetObject;
            }
            return null;
        }

        private string GetErrorMessageForThrowStatement(object targetObject)
        {
            if (targetObject != null)
            {
                return targetObject.ToString();
            }
            return "ScriptHalted";
        }

        public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
        {
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
        {
            try
            {
                tryStatementAst.Body.Visit(this);
            }
            catch (FlowControlException)
            {
                // flow control (return, exit, continue, break) is nothing the user should see and is propagated
                throw;
            }
            catch (Exception ex)
            {
                SetUnderscoreVariable(ex);

                tryStatementAst.CatchClauses.Last().Body.Visit(this);
            }

            return AstVisitAction.SkipChildren;
        }

        private void SetUnderscoreVariable(Exception ex)
        {
            ErrorRecord rec = null;
            if (ex is IContainsErrorRecord)
            {
                rec = ((IContainsErrorRecord)ex).ErrorRecord;
            }
            else
            {
                rec = new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, null);
            }
            ExecutionContext.SetVariable("_", rec);
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            throw new NotImplementedException(); //VisitTypeConstraint(typeConstraintAst);
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            throw new NotImplementedException(); //VisitUsingExpression(usingExpressionAst);
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            this._pipelineCommandRuntime.OutputStream.Write(typeExpressionAst.TypeName.GetReflectionType());
            return AstVisitAction.SkipChildren;
        }
        #endregion
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
using Extensions.Enumerable;
using System.Text.RegularExpressions;
using Pash.ParserIntrinsics;
using System.IO;

namespace System.Management.Pash.Implementation
{
    class ExecutionVisitor : AstVisitor
    {
        readonly ExecutionContext _context;
        readonly PipelineCommandRuntime _pipelineCommandRuntime;
        readonly bool _writeSideEffectsToPipeline;

        public ExecutionVisitor(ExecutionContext context, PipelineCommandRuntime pipelineCommandRuntime, bool writeSideEffectsToPipeline = false)
        {
            this._context = context;
            this._pipelineCommandRuntime = pipelineCommandRuntime;
            this._writeSideEffectsToPipeline = writeSideEffectsToPipeline;
        }

        ExecutionVisitor CloneSub(bool writeSideEffectsToPipeline)
        {
            return new ExecutionVisitor(
                this._context.CreateNestedContext(),
                new PipelineCommandRuntime(this._pipelineCommandRuntime.pipelineProcessor),
                writeSideEffectsToPipeline
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

            if (leftOperand is PSObject) leftOperand = ((PSObject)leftOperand).BaseObject;
            if (rightOperand is PSObject) rightOperand = ((PSObject)rightOperand).BaseObject;

            int? leftOperandInt = leftOperand is int ? ((int?)leftOperand) : null;
            int? rightOperandInt = rightOperand is int ? ((int?)rightOperand) : null;

            bool? leftOperandBool = leftOperand is bool ? ((bool?)leftOperand) : null;
            bool? rightOperandBool = rightOperand is bool ? ((bool?)rightOperand) : null;

            switch (binaryExpressionAst.Operator)
            {
                case TokenKind.DotDot:
                    return Range((int)leftOperand, (int)rightOperand);

                case TokenKind.Plus:
                    return Add(leftOperand, rightOperand);

                case TokenKind.Ieq:
                    if (leftOperand is string)
                        return String.Equals(leftOperand as string, rightOperand as string, StringComparison.InvariantCultureIgnoreCase);
                    return Object.Equals(leftOperand, rightOperand);

                case TokenKind.Ine:
                    if (leftOperandInt.HasValue) return leftOperandInt != rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Igt:
                    if (leftOperandInt.HasValue) return leftOperandInt > rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Ige:
                    if (leftOperandInt.HasValue) return leftOperandInt >= rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Or:
                    if (leftOperandBool.HasValue) return leftOperandBool.Value || rightOperandBool.Value;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Xor:
                    if (leftOperandBool.HasValue) return leftOperandBool != rightOperandBool;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.And:
                    if (leftOperandBool.HasValue) return leftOperandBool.Value && rightOperandBool.Value;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Ilt:
                    if (leftOperandInt.HasValue) return leftOperandInt < rightOperandInt;
                    throw new NotImplementedException(binaryExpressionAst.ToString());

                case TokenKind.Ile:
                    if (leftOperandInt.HasValue) return leftOperandInt <= rightOperandInt;
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
                case TokenKind.Band:
                case TokenKind.Bor:
                case TokenKind.Bxor:
                case TokenKind.Join:
                case TokenKind.Ilike:
                case TokenKind.Inotlike:
                case TokenKind.Imatch:
                    return Match(leftOperand, rightOperand);

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

        private bool Match(object leftOperand, object rightOperand)
        {
            if (!(leftOperand is string) || !(rightOperand is string))
                throw new NotImplementedException(string.Format("{0} -match {1}", leftOperand, rightOperand));

            Regex regex = new Regex((string)rightOperand, RegexOptions.IgnoreCase);
            Match match = regex.Match((string)leftOperand);

            SetMatchesVariable(regex, match);

            return match.Success;
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

            _context.SetVariable("Matches", PSObject.AsPSObject(matches));
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
            if (commandAst.InvocationOperator == TokenKind.Dot)
                return VisitDotSourceCommand(commandAst);

            // Pipeline uses global execution context, so we should set its WriteSideEffects flag, and restore it to previous value after.
            var pipeLineContext = _context.CurrentRunspace.ExecutionContext;
            bool writeSideEffects = pipeLineContext.WriteSideEffectsToPipeline;
            try
            {
                pipeLineContext.WriteSideEffectsToPipeline = _writeSideEffectsToPipeline;
                var pipeline = _context.CurrentRunspace.CreateNestedPipeline();

                pipeline.Input.Write(_context.inputStreamReader.ReadToEnd(), true);

                var command = GetCommand(commandAst);

                commandAst.CommandElements
                    // the first CommandElements is the command itself. The rest are parameters/arguments
                    .Skip(1)
                    .Select(ConvertCommandElementToCommandParameter)
                    .ForEach(command.Parameters.Add);

                pipeline.Commands.Add(command);

                _context.PushPipeline(pipeline);
                try
                {
                    // TODO: develop a rational model for null/singleton/collection
                    var result = pipeline.Invoke();
                    if (result.Any())
                    {
                        _pipelineCommandRuntime.WriteObject(result, true);
                    }
                }
                finally
                {
                    _context.PopPipeline();
                }
            }
            finally
            {
                pipeLineContext.WriteSideEffectsToPipeline = writeSideEffects;
            }

            return AstVisitAction.SkipChildren;
        }

        private AstVisitAction VisitDotSourceCommand(CommandAst commandAst)
        {
            object scriptFileName = EvaluateAst(commandAst.Children.First());
            ScriptBlockAst ast = PowerShellGrammar.ParseInteractiveInput(File.ReadAllText(scriptFileName.ToString()));
            ast.Visit(this);

            return AstVisitAction.SkipChildren;
        }

        CommandParameter ConvertCommandElementToCommandParameter(CommandElementAst commandElement)
        {
            if (commandElement is CommandParameterAst)
            {
                var commandParameterAst = commandElement as CommandParameterAst;
                return new CommandParameter(commandParameterAst.ParameterName, commandParameterAst.Argument);
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
            if (commandAst.CommandElements.First() is ScriptBlockExpressionAst)
            {
                var scriptBlockAst = (commandAst.CommandElements.First() as ScriptBlockExpressionAst).ScriptBlock;
                return new Command(scriptBlockAst);
            }

            else
            {
                return new Command(commandAst.GetCommandName());
            }
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
            return EvaluateAst(expressionAst, true);
        }

        object EvaluateAst(Ast expressionAst, bool writeSideEffectsToPipeline)
        {
            var subVisitor = this.CloneSub(writeSideEffectsToPipeline);
            expressionAst.Visit(subVisitor);
            var result = subVisitor._pipelineCommandRuntime.outputResults.Read();

            if (result.Count == 0) return null;
            if (result.Count == 1) return result.Single();
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

                pipelineElement.Visit(new ExecutionVisitor(subContext, subRuntime, this._writeSideEffectsToPipeline));
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
            this._pipelineCommandRuntime.WriteObject(variable.Value, true);

            return AstVisitAction.SkipChildren;
        }

        private PSVariable GetVariable(VariableExpressionAst variableExpressionAst)
        {
            var variable = this._context.SessionState.PSVariable.Get(variableExpressionAst.VariablePath.UserPath);

            return variable;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            var rightValue = EvaluateAst(assignmentStatementAst.Right);
            object newValue = rightValue;

            ExpressionAst expressionAst = assignmentStatementAst.Left;
            var variableExpressionAst = expressionAst as VariableExpressionAst;
            if (variableExpressionAst == null) throw new NotImplementedException(expressionAst.ToString());

            if (assignmentStatementAst.Operator == TokenKind.Equals)
            {
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, rightValue);
            }

            else if (assignmentStatementAst.Operator == TokenKind.PlusEquals)
            {
                dynamic currentValue = _context.GetVariableValue(variableExpressionAst.VariablePath.UserPath);
                dynamic assignmentValue = ((PSObject)rightValue).BaseObject;
                newValue = currentValue + assignmentValue;
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, newValue);
            }

            else if (assignmentStatementAst.Operator == TokenKind.MinusEquals)
            {
                dynamic currentValue = _context.GetVariableValue(variableExpressionAst.VariablePath.UserPath);
                dynamic assignmentValue = ((PSObject)rightValue).BaseObject;
                newValue = currentValue - assignmentValue;
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, newValue);
            }

            else if (assignmentStatementAst.Operator == TokenKind.MultiplyEquals)
            {
                dynamic currentValue = _context.GetVariableValue(variableExpressionAst.VariablePath.UserPath);
                dynamic assignmentValue = ((PSObject)rightValue).BaseObject;
                newValue = currentValue * assignmentValue;
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, newValue);
            }

            else if (assignmentStatementAst.Operator == TokenKind.DivideEquals)
            {
                dynamic currentValue = _context.GetVariableValue(variableExpressionAst.VariablePath.UserPath);
                dynamic assignmentValue = ((PSObject)rightValue).BaseObject;
                newValue = currentValue / assignmentValue;
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, newValue);
            }

            else if (assignmentStatementAst.Operator == TokenKind.RemainderEquals)
            {
                dynamic currentValue = _context.GetVariableValue(variableExpressionAst.VariablePath.UserPath);
                dynamic assignmentValue = ((PSObject)rightValue).BaseObject;
                newValue = currentValue % assignmentValue;
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, newValue);
            }

            if (this._writeSideEffectsToPipeline) this._pipelineCommandRuntime.WriteObject(newValue);

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

            object index = EvaluateAst(indexExpressionAst.Index);

            if (targetValue is PSObject) targetValue = ((PSObject)targetValue).BaseObject;

            var stringTargetValue = targetValue as string;
            if (stringTargetValue != null)
            {
                var result = stringTargetValue[(int)index];
                this._pipelineCommandRuntime.WriteObject(result);
            }

            else if (targetValue is IList)
            {
                var result = (targetValue as IList)[(int)index];
                this._pipelineCommandRuntime.WriteObject(result);
            }

            else if (targetValue is IDictionary)
            {
                var result = (targetValue as IDictionary)[index];
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

                // null is false
                if (condition == null) continue;

                else if (condition is IList && ((IList)condition).Count == 0) continue;

                else if (condition is PSObject)
                {
                    var baseObject = ((PSObject)condition).BaseObject;

                    if (baseObject is bool && ((bool)baseObject) == false) continue;
                }

                else throw new NotImplementedException(clause.Item1.ToString());

                this._pipelineCommandRuntime.WriteObject(EvaluateAst(clause.Item2));
                return AstVisitAction.SkipChildren;
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

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            var expression = methodCallAst.Expression;

            ObjectInfo objectInfo = GetObjectInfo(expression);

            var arguments = methodCallAst.Arguments.Select(EvaluateAst).Select(o => o is PSObject ? ((PSObject)o).BaseObject : o);

            if (methodCallAst.Member is StringConstantExpressionAst)
            {
                BindingFlags bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                var name = (methodCallAst.Member as StringConstantExpressionAst).Value;
                var method = objectInfo.Type.GetMethod(name, bindingFlags, null, arguments.Select(a => a.GetType()).ToArray(), null);
                var result = method.Invoke(objectInfo.Object, arguments.ToArray());

                _pipelineCommandRuntime.WriteObject(result);
                return AstVisitAction.SkipChildren;
            }

            throw new NotImplementedException(this.ToString());
        }

        private ObjectInfo GetObjectInfo(ExpressionAst expression)
        {
            return new ObjectInfo(EvaluateAst(expression));
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            var expression = memberExpressionAst.Expression;

            ObjectInfo objectInfo = GetObjectInfo(expression);

            if (memberExpressionAst.Member is StringConstantExpressionAst)
            {
                object result = null;
                var name = (memberExpressionAst.Member as StringConstantExpressionAst).Value;

                BindingFlags bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                // TODO: Single() is a problem for overloaded methods
                MemberInfo memberInfo = null;

                if (memberExpressionAst.Static)
                {
                    memberInfo = objectInfo.Type.GetMember(name, bindingFlags).Single();
                }
                else
                {
                    memberInfo = objectInfo.Object.GetType().GetMember(name, bindingFlags).Single();
                }

                if (memberInfo != null)
                {
                    switch (memberInfo.MemberType)
                    {
                        case MemberTypes.Field:
                            result = ((FieldInfo)memberInfo).GetValue(objectInfo.Object);
                            break;

                        case MemberTypes.Property:
                            result = ((PropertyInfo)memberInfo).GetValue(objectInfo.Object, null);
                            break;

                        default:
                            throw new NotImplementedException(this.ToString());
                    }
                }

                _pipelineCommandRuntime.WriteObject(result);
                return AstVisitAction.SkipChildren;
            }

            throw new NotImplementedException(this.ToString());
        }

        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            _pipelineCommandRuntime.WriteObject(arrayLiteralAst.Elements.Select(EvaluateAst).ToArray(), true);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            var childVariableExpressionAst = unaryExpressionAst.Child as VariableExpressionAst;
            var childVariable = childVariableExpressionAst == null ? null : GetVariable(childVariableExpressionAst);
            var childVariableValue = childVariable == null ? null : childVariable.Value;

            switch (unaryExpressionAst.TokenKind)
            {
                case TokenKind.PostfixPlusPlus:

                    if (childVariable == null) throw new NotImplementedException(unaryExpressionAst.ToString());
                    if (childVariableValue is PSObject)
                    {
                        if (this._writeSideEffectsToPipeline) this._pipelineCommandRuntime.WriteObject(childVariable.Value);
                        childVariable.Value = PSObject.AsPSObject(((int)((PSObject)childVariableValue).BaseObject) + 1);
                    }
                    else throw new NotImplementedException(childVariableValue.ToString());

                    break;

                case TokenKind.PlusPlus:

                    if (childVariable == null) throw new NotImplementedException(unaryExpressionAst.ToString());
                    if (childVariableValue is PSObject)
                    {
                        childVariable.Value = PSObject.AsPSObject(((int)((PSObject)childVariableValue).BaseObject) + 1);
                        if (this._writeSideEffectsToPipeline) this._pipelineCommandRuntime.WriteObject(childVariable.Value);
                    }
                    else throw new NotImplementedException(childVariableValue.ToString());

                    break;

                case TokenKind.Not:

                    if (childVariable == null) throw new NotImplementedException(unaryExpressionAst.ToString());

                    VisitUnaryNotVariableExpression(childVariable);

                    break;

                default:
                    throw new NotImplementedException(unaryExpressionAst.ToString());
            }

            return AstVisitAction.SkipChildren;
        }

        private void VisitUnaryNotVariableExpression(PSVariable childVariable)
        {
            object childVariableValue = childVariable.GetBaseObjectValue();
            if (childVariableValue is bool)
            {
                this._pipelineCommandRuntime.WriteObject(!(bool)childVariableValue);
            }
            else throw new NotImplementedException(childVariable.Value.ToString());
        }

        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            foreach (var statement in arrayExpressionAst.SubExpression.Statements)
            {
                var value = EvaluateAst(statement, false);
                this._pipelineCommandRuntime.WriteObject(value, true);
            }

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
            Type type = convertExpressionAst.Type.TypeName.GetReflectionType();

            var value = EvaluateAst(convertExpressionAst.Child);

            if (type.IsEnum)
            {
                var result = Enum.Parse(type, (string)value);

                this._pipelineCommandRuntime.WriteObject(result);
                return AstVisitAction.SkipChildren;
            }

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
            if (exitStatementAst.Pipeline == null) throw new ReturnException();

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
            object enumerable = EvaluateAst(forEachStatementAst.Condition);
            IEnumerator enumerator = GetEnumerator(enumerable);

            while (enumerator.MoveNext())
            {
                this._context.SessionState.PSVariable.Set(forEachStatementAst.Variable.VariablePath.UserPath, enumerator.Current);
                _pipelineCommandRuntime.WriteObject(EvaluateAst(forEachStatementAst.Body), true);
            }

            return AstVisitAction.SkipChildren;
        }

        private IEnumerator GetEnumerator(object obj)
        {
            if (obj is PSObject)
            {
                obj = ((PSObject)obj).BaseObject;
            }

            return _pipelineCommandRuntime.GetEnumerator(obj);
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

            EvaluateAst(forStatementAst.Initializer);

            while ((bool)((PSObject)EvaluateAst(forStatementAst.Condition)).BaseObject)
            {
                this._pipelineCommandRuntime.WriteObject(EvaluateAst(forStatementAst.Body), true);
                EvaluateAst(forStatementAst.Iterator);
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
            var value = EvaluateAst(parenExpressionAst.Pipeline);
            this._pipelineCommandRuntime.WriteObject(value, true);
            return AstVisitAction.SkipChildren;
        }

        class ReturnException : Exception
        {
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            if (returnStatementAst.Pipeline == null) throw new ReturnException();

            throw new NotImplementedException(); //VisitReturnStatement(returnStatementAst);
        }

        public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            try
            {
                scriptBlockAst.EndBlock.Visit(this);
            }
            catch (ReturnException)
            {
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
            this._pipelineCommandRuntime.outputResults.Write(stringConstantExpressionAst.Value);
            return AstVisitAction.SkipChildren;
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
            try
            {
                tryStatementAst.Body.Visit(this);
            }
            catch (ReturnException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, null);
                _context.SetVariable("_", error);

                tryStatementAst.CatchClauses.Last().Body.Visit(this);
            }

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            throw new NotImplementedException(); //VisitTypeConstraint(typeConstraintAst);
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
            this._pipelineCommandRuntime.outputResults.Write(typeExpressionAst.TypeName.GetReflectionType());
            return AstVisitAction.SkipChildren;
        }
        #endregion
    }
}

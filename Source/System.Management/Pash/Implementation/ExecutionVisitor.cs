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
        readonly ExecutionContext _context;
        readonly PipelineCommandRuntime _pipelineCommandRuntime;
        readonly bool _writeSideEffectsToPipeline;
        readonly HashSet<string> _hashtableAccessibleMembers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
            "Count", "Keys", "Values", "Remove"
        };

        public ExecutionVisitor(ExecutionContext context, PipelineCommandRuntime pipelineCommandRuntime, bool writeSideEffectsToPipeline = false)
        {
            this._context = context;
            this._pipelineCommandRuntime = pipelineCommandRuntime;
            this._writeSideEffectsToPipeline = writeSideEffectsToPipeline;
        }

        ExecutionVisitor CloneSub(bool writeSideEffectsToPipeline)
        {
            var subContext = this._context.CreateNestedContext();
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

                case TokenKind.Band:
                    return BitwiseOperation.And(leftOperand, rightOperand);
                case TokenKind.Bor:
                    return BitwiseOperation.Or(leftOperand, rightOperand);
                case TokenKind.Bxor:
                    return BitwiseOperation.Xor(leftOperand, rightOperand);
                case TokenKind.Imatch:
                    return Match(leftOperand, rightOperand);

                case TokenKind.Multiply:
                    return Multiply(leftOperand, rightOperand);

                case TokenKind.Divide:
                    return Divide(leftOperand, rightOperand);

                case TokenKind.Minus:
                    return Subtract(leftOperand, rightOperand);

                case TokenKind.Rem:
                    return Remainder(leftOperand, rightOperand);

                case TokenKind.Equals:
                case TokenKind.PlusEquals:
                case TokenKind.MinusEquals:
                case TokenKind.MultiplyEquals:
                case TokenKind.DivideEquals:
                case TokenKind.RemainderEquals:
                case TokenKind.Format:
                case TokenKind.Not:
                case TokenKind.Bnot:
                case TokenKind.Join:
                case TokenKind.Ilike:
                case TokenKind.Inotlike:
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

        private object Add(object leftValuePacked, object rightValuePacked)
        {
            var leftValue = PSObject.Unwrap(leftValuePacked);
            var rightValue = PSObject.Unwrap(rightValuePacked);

            ////  7.7.1 Addition
            ////      Description:
            ////      
            ////          The result of the addition operator + is the sum of the values designated by the two operands after the usual arithmetic conversions (§6.15) have been applied.
            ////      
            ////          This operator is left associative.
            ////      
            ////      Examples: See ReferenceTests.AdditiveOperatorTests_7_7
            ////      
            ////          12 + -10L               # long result 2
            ////          -10.300D + 12           # decimal result 1.700
            ////          10.6 + 12               # double result 22.6
            ////          12 + "0xabc"            # int result 2760

            // string concatenation 7.7.2
            if (leftValue is string)
            {
                if (rightValue is object[])
                {
                    return leftValue + String.Join(" ", (object[])rightValue);
                }
                return leftValue + rightValue.ToString();
            }
            // array concatenation (7.7.3)
            if (leftValue is Array)
            {
                var resultList = new List<object>();
                var enumerable = LanguagePrimitives.GetEnumerable(rightValuePacked);
                foreach (var el in ((Array) leftValue))
                {
                    resultList.Add(el);
                }
                if (enumerable == null)
                {
                    resultList.Add(rightValuePacked);
                }
                else
                {
                    foreach (var el in enumerable)
                    {
                        resultList.Add(el);
                    }
                }
                return resultList.ToArray();
            }
            // hashtable concatenation (7.7.4)
            if (leftValue is Hashtable && rightValue is Hashtable)
            {
                var resultHash = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
                var leftHash = (Hashtable) leftValue;
                var rightHash = (Hashtable) rightValue;
                foreach (var key in leftHash.Keys)
                {
                    resultHash[key] = leftHash[key];
                }
                foreach (var key in rightHash.Keys)
                {
                    if (resultHash.ContainsKey(key))
                    {
                        var fmt = "Cannot concat hashtables: The key '{0}' is already used in the left Hashtable";
                        throw new InvalidOperationException(String.Format(fmt, key.ToString()));
                    }
                    resultHash[key] = rightHash[key];
                }
                return resultHash;
            }

            // arithmetic expression (7.7.1)
            Func<dynamic, dynamic, dynamic> addOp = (dynamic x, dynamic y) => checked(x + y);
            return ArithmeticOperation(leftValue, rightValue, "+", addOp);
        }

        private object Multiply(object leftValue, object rightValue)
        {
            // string replication (7.6.2)
            if (leftValue is string)
            {
                long num;
                if (!LanguagePrimitives.TryConvertTo<long>(rightValue, out num))
                {
                    ThrowInvalidArithmeticOperationException(leftValue, rightValue, "*");
                }
                var sb = new StringBuilder();
                for (int i = 0; i < num; i++)
                {
                    sb.Append(leftValue);
                }
                return sb.ToString();
            }

            // array replication (7.6.3)
            if (leftValue is Array)
            {
                long num;
                if (!LanguagePrimitives.TryConvertTo<long>(rightValue, out num))
                {
                    ThrowInvalidArithmeticOperationException(leftValue, rightValue, "*");
                }
                var leftArray = (Array)leftValue;
                long length = leftArray.Length;
                var resultArray = Array.CreateInstance(leftArray.GetType().GetElementType(), length * num);
                for (long i = 0; i < num; i++)
                {
                    Array.Copy(leftArray, 0, resultArray, i * length, length);
                }
                return resultArray;
            }

            // arithmetic expression (7.6.1)
            Func<dynamic, dynamic, dynamic> mulOp = (dynamic x, dynamic y) => checked(x * y);
            return ArithmeticOperation(leftValue, rightValue, "*", mulOp);
        }

        private object Divide(object leftValue, object rightValue)
        {
            // arithmetic division (7.6.4)
            object convLeft, convRight;
            if (!LanguagePrimitives.UsualArithmeticConversion(leftValue, rightValue, 
                                                              out convLeft, out convRight))
            {
                ThrowInvalidArithmeticOperationException(leftValue, rightValue, "/");
            }
            dynamic left = convLeft;
            dynamic right = convRight;
            // we are overflow safe for ints and longs, but need to check if the result
            // needs to be double
            if ((convLeft is int || convLeft is long) && (convRight is int || convRight is long))
            {
                if (left % right == 0)
                {
                    return left / right;
                }
                // otherwise we have a remainder, convert to double
                left = (double)left;
                right = (double)right;
            }
            return left / right;
        }

        private object Remainder(object leftValue, object rightValue)
        {
            // arithmetic remainder (7.6.5)
            Func<dynamic, dynamic, dynamic> remOp = (dynamic x, dynamic y) => checked(x % y);
            return ArithmeticOperation(leftValue, rightValue, "%", remOp);
        }

        private object Subtract(object leftValue, object rightValue)
        {
            // arithmetic expression (7.7.5)
            Func<dynamic, dynamic, dynamic> subOp = (dynamic x, dynamic y) => checked(x - y);
            return ArithmeticOperation(leftValue, rightValue, "-", subOp);
        }

        private void ThrowInvalidArithmeticOperationException(object left, object right, string op)
        {
            var msg = String.Format("Operation [{0}] {1} [{2}] is not defined",
                                    left.GetType().FullName, op,
                                    right.GetType().FullName);
            throw new PSInvalidOperationException(msg);
        }

        private object ArithmeticOperation(object leftUnconverted, object rightUnconverted, string op,
                                           Func<dynamic, dynamic, dynamic> checkedOperation)
        {
            object left, right;
            if (!LanguagePrimitives.UsualArithmeticConversion(leftUnconverted, rightUnconverted, 
                                                              out left, out right))
            {
                ThrowInvalidArithmeticOperationException(leftUnconverted, rightUnconverted, op);
            }
            if (left is int && right is int)
            {
                try
                {
                    return checkedOperation(left, right);
                }
                catch (OverflowException)
                {
                    left = (long)((int) left); // int cast -> object to int, then to long
                    right = (long)((int) right);
                }
            }
            if ((left is int || left is long) && (right is int || right is long))
            {
                try
                {
                    return checkedOperation(left, right);
                }
                catch (OverflowException)
                {
                    // first dynamic cast, as the objects might be ints or longs
                    left = (double) ((dynamic) left);
                    right = (double) ((dynamic)right);
                }
            }
            // otherwise its float, double, decimal, which cannot overflow
            return checkedOperation(left, right);
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
            var pipeLineContext = _context.CurrentRunspace.ExecutionContext;
            bool writeSideEffects = pipeLineContext.WriteSideEffectsToPipeline;
            try
            {
                pipeLineContext.WriteSideEffectsToPipeline = _writeSideEffectsToPipeline;
                var pipeline = _context.CurrentRunspace.CreateNestedPipeline();
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
                    foreach (var input in _context.InputStream.Read())
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

                    pipeline.Commands.Add(command);
                }

                // now execute the pipeline
                _context.PushPipeline(pipeline);
                try
                {
                    var results = pipeline.Invoke();
                    // read output and error and write them as results of the current commandRuntime
                    foreach (var curResult in results)
                    {
                        _pipelineCommandRuntime.WriteObject(curResult);
                    }
                    var errors = pipeline.Error.NonBlockingRead();
                    foreach (var curError in errors)
                    {
                        _pipelineCommandRuntime.ErrorStream.Write(curError);
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
            if (variableExpressionAst.VariablePath.IsDriveQualified)
            {
                VisitDriveQualifiedVariableExpression(variableExpressionAst);
            }
            else
            {
                var variable = GetVariable(variableExpressionAst);
                var value = (variable != null) ? variable.Value : null;
                this._pipelineCommandRuntime.WriteObject(value);
            }

            return AstVisitAction.SkipChildren;
        }

        private void VisitDriveQualifiedVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            SessionStateProviderBase provider = GetSessionStateProvider(variableExpressionAst.VariablePath);
            if (provider != null)
            {
                var path = new Path(variableExpressionAst.VariablePath.GetUnqualifiedUserPath());
                object item = provider.GetSessionStateItem(path);
                object value = provider.GetValueOfItem(item);
                _pipelineCommandRuntime.WriteObject(value);
            }
        }

        private SessionStateProviderBase GetSessionStateProvider(VariablePath variablePath)
        {
            PSDriveInfo driveInfo = _context.SessionState.Drive.Get(variablePath.DriveName);
            if (driveInfo != null)
            {
                return _context.SessionStateGlobal.GetProviderInstance(driveInfo.Provider.Name) as SessionStateProviderBase;
            }
            return null;
        }

        private PSVariable GetVariable(VariableExpressionAst variableExpressionAst)
        {
            var variable = this._context.SessionState.PSVariable.Get(variableExpressionAst.VariablePath.UserPath);

            return variable;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            ExpressionAst expressionAst = assignmentStatementAst.Left;
            var isEquals = assignmentStatementAst.Operator == TokenKind.Equals;
            var isVariableAssignment = expressionAst is VariableExpressionAst;
            var rightValueRes = EvaluateAst(assignmentStatementAst.Right);
            // a little ugly, but we need to stay dynamic. It's crucial that a psobject isn't unpacked if it's simply
            // assigned to a variable, otherwise we could lose some important properties
            // TODO: this is more like a workaround. PSObject should implement implicit casting,
            // then no checks and .BaseObject calls should be necessary anymore
            bool unpackPSObject = !isEquals || !isVariableAssignment;
            dynamic rightValue = (rightValueRes is PSObject && unpackPSObject) ?
                ((PSObject)rightValueRes).BaseObject : rightValueRes;
            object newValue = rightValue;

            dynamic currentValueRes = isEquals ? null : EvaluateAst(assignmentStatementAst.Left);
            dynamic currentValue = (currentValueRes != null && currentValueRes is PSObject && unpackPSObject) ?
                                   ((PSObject)currentValueRes).BaseObject : currentValueRes;

            if (assignmentStatementAst.Operator == TokenKind.Equals)
            {
                newValue = rightValue;
            }
            else if (assignmentStatementAst.Operator == TokenKind.PlusEquals)
            {
                newValue = currentValue + rightValue;
            }
            else if (assignmentStatementAst.Operator == TokenKind.MinusEquals)
            {
                newValue = currentValue - rightValue;
            }
            else if (assignmentStatementAst.Operator == TokenKind.MultiplyEquals)
            {
                newValue = currentValue * rightValue;
            }
            else if (assignmentStatementAst.Operator == TokenKind.DivideEquals)
            {
                newValue = currentValue / rightValue;
            }
            else if (assignmentStatementAst.Operator == TokenKind.RemainderEquals)
            {
                newValue = currentValue % rightValue;
            }
            else
            {
                throw new NotImplementedException("Unsupported operator: " + assignmentStatementAst.Operator.ToString());
            }

            if (this._writeSideEffectsToPipeline)
            {
                this._pipelineCommandRuntime.WriteObject(newValue);
            }

            if (isVariableAssignment)
            {
                SetVariableValue((VariableExpressionAst)expressionAst, newValue);
            }
            else if (expressionAst is MemberExpressionAst)
            {
                SetMemberExpressionValue((MemberExpressionAst)expressionAst, newValue);
            }
            else if (expressionAst is IndexExpressionAst)
            {
                SetIndexExpressionValue((IndexExpressionAst)expressionAst, newValue);
            }
            else
            {
                var msg = String.Format("The expression type '{0}' is currently not supported for assignments",
                                        expressionAst.GetType().ToString());
                throw new NotImplementedException(msg);
            }

            return AstVisitAction.SkipChildren;
        }

        private void SetVariableValue(VariableExpressionAst variableExpressionAst, object value)
        {
            if (variableExpressionAst.VariablePath.IsDriveQualified)
            {
                SetDriveVariableValue(variableExpressionAst, value);
            }
            else
            {
                _context.SetVariable(variableExpressionAst.VariablePath.UserPath, value);
            }
        }

        private void SetDriveVariableValue(VariableExpressionAst variableExpressionAst, object value)
        {
            SessionStateProviderBase provider = GetSessionStateProvider(variableExpressionAst.VariablePath);
            if (provider != null)
            {
                var path = new Path(variableExpressionAst.VariablePath.GetUnqualifiedUserPath());
                provider.SetSessionStateItem(path, value, false);
            }
        }

        private void SetMemberExpressionValue(MemberExpressionAst memberExpressionAst, object value)
        {
            string memberName;
            var psobj = EvaluateAsPSObject(memberExpressionAst.Expression);
            var unwraped = PSObject.Unwrap(psobj);
            var memberNameObj = EvaluateAst(memberExpressionAst.Member, false);
            // check for Hashtable first
            if (unwraped is Hashtable && memberNameObj != null &&
                !_hashtableAccessibleMembers.Contains(memberNameObj.ToString()))
            { 
                ((Hashtable) unwraped)[memberNameObj] = value;
                return;
            }
            // else it's a PSObject
            var member = GetPSObjectMember(psobj, memberNameObj, memberExpressionAst.Static, out memberName);
            if (member == null)
            {
                throw new PSArgumentNullException(String.Format("Member '{0}' to be assigned is null", memberName));
            }
            member.Value = value;
        }

        private void SetIndexExpressionValue(IndexExpressionAst indexExpressionAst, object value)
        {
            var target = PSObject.Unwrap(EvaluateAst(indexExpressionAst.Target));
            var index = PSObject.Unwrap(EvaluateAst(indexExpressionAst.Index, false));
            var arrayTarget = target as Array;
            if (arrayTarget != null && arrayTarget.Rank > 1)
            {
                var rawIndices = (int[])LanguagePrimitives.ConvertTo(index, typeof(int[]));
                var indices = ConvertNegativeIndicesForMultidimensionalArray(arrayTarget, rawIndices);
                var convertedValue = LanguagePrimitives.ConvertTo(value, arrayTarget.GetType().GetElementType());
                arrayTarget.SetValue(convertedValue, indices);
                return;
            }
            // we don't support assignment to slices (, yet?), so throw an error
            if (index is Array && ((Array)index).Length > 1)
            {
                throw new PSInvalidOperationException("Assignment to slices is not supported");
            }
            // otherwise do assignments
            var iListTarget = target as IList;
            if (iListTarget != null) // covers also arrays
            {
                var intIdx = (int)LanguagePrimitives.ConvertTo(index, typeof(int));
                intIdx = intIdx < 0 ? intIdx + iListTarget.Count : intIdx;
                iListTarget[intIdx] = value;
            }
            else if (target is IDictionary)
            {
                ((IDictionary)target)[index] = value;
            }
            else
            {
                var msg = String.Format("Cannot set index for type '{0}'", target.GetType().FullName);
                throw new PSInvalidOperationException(msg);
            }
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            _context.SessionState.Function.Set(functionDefinitionAst.Name, functionDefinitionAst.Body.GetScriptBlock());
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            var targetValue = PSObject.Unwrap(EvaluateAst(indexExpressionAst.Target, true));

            object index = PSObject.Unwrap(EvaluateAst(indexExpressionAst.Index));

            if (targetValue is PSObject) targetValue = ((PSObject)targetValue).BaseObject;

            // Check dicts/hashtables first
            var dictTarget = targetValue as IDictionary;
            if (dictTarget != null)
            {
                var idxobjects = index is object[] ? (object[])index : new object[] { index };
                foreach (var idxobj in idxobjects)
                {
                    var res = dictTarget[PSObject.Unwrap(idxobj)];
                    _pipelineCommandRuntime.WriteObject(res);
                }
                return AstVisitAction.SkipChildren;
            }

            // otherwise we need int indexing
            var indices = (int[]) LanguagePrimitives.ConvertTo(index, typeof(int[]));

            // check for real multidimensional array access first
            var arrayTarget = targetValue as Array;
            if (arrayTarget != null && arrayTarget.Rank > 1)
            {
                int[] convIndices = ConvertNegativeIndicesForMultidimensionalArray(arrayTarget, indices);
                object arrayValue = null;
                try
                {
                    arrayValue = arrayTarget.GetValue(convIndices);
                }
                catch (IndexOutOfRangeException) // just write nothing to pipeline
                {
                }
                this._pipelineCommandRuntime.WriteObject(arrayValue);
                return AstVisitAction.SkipChildren;
            }

            // if we have a string, we need to access it as an char[]
            if (targetValue is string)
            {
                targetValue = ((string)targetValue).ToCharArray();
            }

            // now we can access arrays, list, and string (charArrays) all the same by using the IList interface
            var iListTarget = targetValue as IList;
            if (iListTarget == null)
            {
                var msg = String.Format("Cannot index an object of type '{0}'.", targetValue.GetType().FullName);
                throw new PSInvalidOperationException(msg);
            }

            var numItems = iListTarget.Count;
            // now get all elements from the index list
            bool writtenFromList = false;
            foreach (int curIdx in indices)
            {
                // support negative indices
                var idx = curIdx < 0 ? curIdx + numItems : curIdx;
                // check if it's a valid index, otherwise ignore
                if (idx >= 0 && idx < numItems)
                {
                    _pipelineCommandRuntime.WriteObject(iListTarget[idx]);
                    writtenFromList = true;
                }
            }
            if (!writtenFromList)
            {
                _pipelineCommandRuntime.WriteObject(null);
            }
            return AstVisitAction.SkipChildren;
        }

        private int[] ConvertNegativeIndicesForMultidimensionalArray(Array array, int[] rawIndices)
        {
            if (array.Rank != rawIndices.Length)
            {
                var msg = String.Format("The index [{0}] is invalid to access an {1}-dimensional array",
                                        String.Join(",", rawIndices), array.Rank);
                throw new PSInvalidOperationException(msg);
            }
            var convIndices = new int[array.Rank];
            for (int i = 0; i < rawIndices.Length; i++)
            {
                convIndices[i] = rawIndices[i] < 0 ? rawIndices[i] + array.GetLength(i) : rawIndices[i];
            }
            return convIndices;
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
            string memberName;
            var psobj = EvaluateAsPSObject(methodCallAst.Expression);
            if (psobj == null)
            {
                throw new PSInvalidOperationException("Cannot invoke a method of a NULL expression");
            }
            var memberNameObj = EvaluateAst(methodCallAst.Member, false);
            var method = GetPSObjectMember(psobj, memberNameObj, methodCallAst.Static, out memberName) as PSMethodInfo;
            if (method == null)
            {
                throw new PSArgumentException(String.Format("The object has no method called '{0}'", memberName));
            }
            var arguments = methodCallAst.Arguments.Select(EvaluateAst).Select(o => o is PSObject ? ((PSObject)o).BaseObject : o);
            var result = method.Invoke(arguments.ToArray());
            if (result != null)
            {
                _pipelineCommandRuntime.WriteObject(PSObject.AsPSObject(result));
            }
            return AstVisitAction.SkipChildren;
        }

        private PSObject EvaluateAsPSObject(ExpressionAst expression)
        {
            // if the expression is a variable including an enumerable object (e.g. collection)
            // then we want the collection itself. The enumerable object should only be expanded when being processed
            // e.g. in a pipeline
            return PSObject.WrapOrNull(EvaluateAst(expression, false));
        }

        private PSMemberInfo GetPSObjectMember(PSObject psobj, object memberNameObj, bool isStatic, out string memberName)
        {

            if (memberNameObj == null)
            {
                throw new PSArgumentNullException("Member name evaluates to null");
            }
            memberName = memberNameObj.ToString();
            if (psobj == null)
            {
                return null;
            }
            // Powershell allows access to hastable values by member acccess
            if (isStatic)
            {
                return psobj.StaticMembers[memberName];
            }
            return psobj.Members[memberName];
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            string memberName;
            var psobj = EvaluateAsPSObject(memberExpressionAst.Expression);
            var unwraped = PSObject.Unwrap(psobj);
            var memberNameObj = EvaluateAst(memberExpressionAst.Member, false);
            // check for Hastable first
            if (unwraped is Hashtable && memberNameObj != null &&
                !_hashtableAccessibleMembers.Contains(memberNameObj.ToString()))
            {
                var hashtable = (Hashtable)unwraped;
                _pipelineCommandRuntime.WriteObject(hashtable[memberNameObj]);
            }
            else // otherwise a PSObject
            {
                var member = GetPSObjectMember(psobj, memberNameObj, memberExpressionAst.Static, out memberName);
                var value = (member == null) ? null : PSObject.AsPSObject(member.Value);
                _pipelineCommandRuntime.WriteObject(value);
            }
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
            return AstVisitAction.SkipChildren;
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
            return AstVisitAction.SkipChildren;
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
            throw new NotImplementedException(); //VisitDoUntilStatement(doUntilStatementAst);
        }

        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            throw new NotImplementedException(); //VisitDoWhileStatement(doWhileStatementAst);
        }

        public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            int exitCode = 0;
            if (exitStatementAst.Pipeline != null)
            {
                var value = EvaluateAst(exitStatementAst.Pipeline, false);
                // Default PS behavior: either convert value to int or it's 0 (see above)
                LanguagePrimitives.TryConvertTo<int>(value, out exitCode);
            }
            throw new ExitException(exitCode);
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
            throw new NotImplementedException(); //VisitFileRedirection(redirectionAst);
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
                this._context.SessionState.PSVariable.Set(forEachStatementAst.Variable.VariablePath.UserPath, enumerator.Current);
                _pipelineCommandRuntime.WriteObject(EvaluateAst(forEachStatementAst.Body, false), true);
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

            while (forStatementAst.Condition != null ? (bool)((PSObject)EvaluateAst(forStatementAst.Condition)).BaseObject : true)
            {
                this._pipelineCommandRuntime.WriteObject(EvaluateAst(forStatementAst.Body, false), true);
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

            // just iterate over children
            return base.VisitNamedBlock(namedBlockAst);
        }

        private AstVisitAction VisitNamedBlockWithTraps(NamedBlockAst namedBlockAst)
        {
            foreach (StatementAst statement in namedBlockAst.Statements)
            {
                try
                {
                    statement.Visit(this);
                }
                catch (ReturnException)
                {
                    throw;
                }
                catch (ExitException)
                {
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
                statement.Visit(this);

                if (statement is ContinueStatementAst)
                {
                    return AstVisitAction.Continue;
                }
                else if (statement is BreakStatementAst)
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
            object error = _context.GetVariableValue("_");
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
            // We execute this in a subcontext, because single enumerable results are 
            // evaluated and its values are written to pipeline (i.e. ". {1,2,3}" writes
            // all three values to pipeline
            // therefore we have to catch exceptions for now to read and write
            // the subVisitors' output stream, so that already written results appear
            // also in this OutputStream
            // TODO: is there a better way? Maybe evaluation of enumerable results should be
            // somewhere else? Can't be when writing the input pipe of a pipeline, because
            // it would enumerates many objects that should be enumerated
            var subVisitor = this.CloneSub(false);
            object returnResult = null;
            Exception exception = null;
            try
            {
                scriptBlockAst.EndBlock.Visit(subVisitor);
            }
            catch (ReturnException e)
            {
                // return exception is okay
                returnResult = e.Value; // we first need to read the other results
            }
            catch (Exception e)
            {
                // others need to be rethrown
                exception = e;
            }
            // now read errrot and output and write in out streams
            foreach (var error in subVisitor._pipelineCommandRuntime.ErrorStream.Read())
            {
                // we need to use ErrorStream.Write instead of WriteError to avoid
                // adding it to the error variable twice!
                _pipelineCommandRuntime.ErrorStream.Write(error);
            }
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
            // rethrow exception if we had one
            if (exception != null)
            {
                throw exception;
            }
            // if the exception was a return excpetion, also write the returnResult, if existing
            if (PSObject.Unwrap(returnResult) != null)
            {
                _pipelineCommandRuntime.WriteObject(returnResult, true);
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
            catch (ReturnException)
            {
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
            var error = new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, null);
            _context.SetVariable("_", error);
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
            /* The controlling expression while-condition must have type bool or
             * be implicitly convertible to that type. The loop body, which
             * consists of statement-block, is executed repeatedly while the
             * controlling expression tests True. The controlling expression
             * is evaluated before each execution of the loop body.
             */
            while ((bool)((PSObject)EvaluateAst(whileStatementAst.Condition)).BaseObject)
            {
                this._pipelineCommandRuntime.WriteObject(EvaluateAst(whileStatementAst.Body, false), true);
            }

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            this._pipelineCommandRuntime.OutputStream.Write(typeExpressionAst.TypeName.GetReflectionType());
            return AstVisitAction.SkipChildren;
        }
        #endregion
    }
}

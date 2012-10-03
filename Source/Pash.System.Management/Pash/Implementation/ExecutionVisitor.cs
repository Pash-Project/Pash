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

            if (binaryExpressionAst.Operator == TokenKind.Plus)
            {
                return Add(leftOperand, rightOperand);
            }

            else if (binaryExpressionAst.Operator == TokenKind.DotDot)
            {
                return Range((int)leftOperand, (int)rightOperand);
            }

            else throw new NotImplementedException(binaryExpressionAst.ToString());
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

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            throw new NotImplementedException(typeExpressionAst.ToString());
        }
    }
}

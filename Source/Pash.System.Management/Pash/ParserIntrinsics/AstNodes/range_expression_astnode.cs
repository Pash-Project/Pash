using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using System.Diagnostics;
using System.Reflection;
using Pash.Implementation;
using System.Management.Automation;
using Extensions.String;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class range_expression_astnode : _astnode
    {
        public range_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////        range_expression:
            ////            array_literal_expression
            ////            range_expression   ..   new_lines_opt   array_literal_expression

            // if only 1 child node, then the default (base) implementation will forward to that child
            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return base.Execute(context, commandRuntime);
            }

            if (parseTreeNode.ChildNodes.Count != 3)
                throw new Exception("unexpected child node count {0}".FormatString(parseTreeNode.ChildNodes.Count));

            var startRangeExpressionAstNode = (_astnode)parseTreeNode.ChildNodes[0].AstNode;

            KeywordTerminal keywordTerminal = (KeywordTerminal)parseTreeNode.ChildNodes[1].Term;
            if (keywordTerminal.Text != "..") throw new NotImplementedException();
            var endRangeExpressionAstNode = (array_literal_expression_astnode)parseTreeNode.ChildNodes[2].AstNode;

            return Execute(context, commandRuntime, startRangeExpressionAstNode, endRangeExpressionAstNode)
                .Select(i=>new PSObject(i))
                .ToArray()
                ;
        }

        private static IEnumerable<int> Execute(ExecutionContext context, ICommandRuntime commandRuntime, _astnode startRangeExpressionNode, _astnode endRangeExpressionNode)
        {
            //// Description:
            ////
            //// A range-expression creates an unconstrained 1-dimensional array whose elements are the values of 
            //// the int sequence specified by the range bounds. The values designated by the operands are converted 
            //// to int, if necessary (§6.4). 
            var leftOperandValue = (int)startRangeExpressionNode.Execute(context, commandRuntime);
            var rightOperandValue = (int)endRangeExpressionNode.Execute(context, commandRuntime);


            //// The operand designating the lower value after conversion is the lower 
            //// bound, while the operand designating the higher value after conversion is the upper bound. 
            if (leftOperandValue < rightOperandValue)
            {
                return Extensions.Enumerable._.Generate(leftOperandValue, i => i + 1, rightOperandValue);
            }

            //// Both bounds may be the same, in which case, the resulting array has length 1. 
            if (leftOperandValue == rightOperandValue) return new[] { leftOperandValue };

            //// If the left operand designates the lower bound, the sequence is in ascending order. If the left 
            //// operand designates the upper bound, the sequence is in descending order.
            if (rightOperandValue < leftOperandValue)
            {
                return Extensions.Enumerable._.Generate(leftOperandValue, i => i - 1, rightOperandValue);
            }

            //// [Note: Conceptually, this operator is a shortcut for the corresponding binary comma operator 
            //// sequence. For example, the range 5..8 can also be generated using 5,6,7,8. However, if an ascending 
            //// or descending sequence is needed without having an array, an implementation may avoid generating an 
            //// actual array. For example, in foreach ($i in 1..5) { … }, no array need be created. end note]
            ////
            //// A range-expression can be used to specify an array slice (§9.9).

            throw new Exception("unreachable");
        }
    }
}

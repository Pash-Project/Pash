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
using System.Collections;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class array_literal_expression_astnode : _astnode
    {
        public readonly unary_expression_astnode UnaryExpression;
        public readonly array_literal_expression_astnode ArrayLiteralExpression;

        public array_literal_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        array_literal_expression:
            ////            unary_expression
            ////            unary_expression   ,    new_lines_opt   array_literal_expression
            this.UnaryExpression = this.ChildAstNodes.First().As<unary_expression_astnode>();

            if (this.ChildAstNodes.Count == 1)
            {
                // do nothing
            }

            else if (this.ChildAstNodes.Count == 3)
            {
                Debug.Assert(this.parseTreeNode.ChildNodes[1].FindTokenAndGetText() == ",", this.ToString());

                this.ArrayLiteralExpression = this.ChildAstNodes[2].As<array_literal_expression_astnode>();
            }

            else throw new InvalidOperationException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.ArrayLiteralExpression == null)
            {
                return this.UnaryExpression.Execute(context, commandRuntime);
            }

            return Execute(context, commandRuntime, UnaryExpression, ArrayLiteralExpression)
                .ToArray()
                ;
        }

        // PERF: This is O(n^2), as we build up the array right-to-left. That would only matter for very large arrays.
        static IEnumerable<object> Execute(ExecutionContext context, ICommandRuntime commandRuntime, unary_expression_astnode firstItemAstNode, array_literal_expression_astnode remainingItemsAstNode)
        {
            //// 7.3 Binary comma operator
            ////
            //// Description:
            ////
            ////     The binary comma operator creates a 1-dimensional array whose elements are the values designated by its operands, in lexical order. The array has unconstrained type.
            ////
            var leftOperandValue = firstItemAstNode.Execute(context, commandRuntime);
            var rightOperandValue = remainingItemsAstNode.Execute(context, commandRuntime);

            var newList = new List<object>() { leftOperandValue };

            if (rightOperandValue is object[])
            {
                newList.AddRange((object[])rightOperandValue);
            }
            else
            {
                newList.Add(rightOperandValue);
            }
            return newList.ToArray();
        }
    }
}

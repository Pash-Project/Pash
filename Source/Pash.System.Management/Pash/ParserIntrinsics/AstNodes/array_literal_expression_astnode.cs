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
        public array_literal_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////        array_literal_expression:
            ////            unary_expression
            ////            unary_expression   ,    new_lines_opt   array_literal_expression

            // if only 1 child node, then the default (base) implementation will forward to that child
            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return base.Execute(context, commandRuntime);
            }

            if (parseTreeNode.ChildNodes.Count != 3)
                throw new Exception("unexpected child node count {0}".FormatString(parseTreeNode.ChildNodes.Count));

            var firstItemAstNode = (_astnode)parseTreeNode.ChildNodes[0].AstNode;

            KeywordTerminal keywordTerminal = (KeywordTerminal)parseTreeNode.ChildNodes[1].Term;
            if (keywordTerminal.Text != ",") throw new NotImplementedException();
            var remainingItemsAstNode = (_astnode)parseTreeNode.ChildNodes[2].AstNode;

            return Execute(context, commandRuntime, firstItemAstNode, remainingItemsAstNode)
                .Select(i => new PSObject(i))
                .ToArray()
                ;
        }

        // PERF: This is O(n^2), as we build up the array right-to-left. That would only matter for very large arrays.
        static IEnumerable<object> Execute(ExecutionContext context, ICommandRuntime commandRuntime, _astnode firstItemAstNode, _astnode remainingItemsAstNode)
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

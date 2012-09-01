
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class expression_with_unary_operator_astnode : _astnode
    {
        public readonly unary_expression_astnode NegativeExpression;

        public expression_with_unary_operator_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        expression_with_unary_operator:
            ////            ,   new_lines_opt   unary_expression
            ////            -not   new_lines_opt   unary_expression
            ////            !   new_lines_opt   unary_expression
            ////            -bnot   new_lines_opt   unary_expression
            ////            +   new_lines_opt   unary_expression
            ////            dash   new_lines_opt   unary_expression
            ////            pre_increment_expression
            ////            pre_decrement_expression
            ////            cast_expression
            ////            -split   new_lines_opt   unary_expression
            ////            -join   new_lines_opt   unary_expression
            ////            dash   new_lines_opt   unary_expression

            if (this.parseTreeNode.ChildNodes[0].Token.Terminal == PowerShellGrammar.Terminals.dash)
            {
                Debug.Assert(this.parseTreeNode.ChildNodes.Count == 2, this.ToString());
                NegativeExpression = this.ChildAstNodes[1].As<unary_expression_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////    7.2.5 Unary minus
            ////    Description:
            ////    
            ////    An expression of the form -unary-expression is treated as if it were written as 0 - unary-expression (§7.7). [Note: The integer literal 0 has type int. end note]
            ////    
            ////    This operator is right associative.

            if (parseTreeNode.ChildNodes.Count == 2)
            {
                var rightValue = (int)this.NegativeExpression.Execute(context, commandRuntime);
                return 0 - rightValue;
            }

            throw new NotImplementedException(parseTreeNode.ToString());
        }
    }
}

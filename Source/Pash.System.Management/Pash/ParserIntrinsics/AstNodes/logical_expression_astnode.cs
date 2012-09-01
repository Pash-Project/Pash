using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class logical_expression_astnode : _astnode
    {
        public readonly bitwise_expression_astnode BitwiseExpression;
 
        public logical_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        logical_expression:
            ////            bitwise_expression
            ////            logical_expression   _and   new_lines_opt   bitwise_expression
            ////            logical_expression   _or   new_lines_opt   bitwise_expression
            ////            logical_expression   _xor   new_lines_opt   bitwise_expression

            if (this.ChildAstNodes.Count == 1)
            {
                this.BitwiseExpression = this.ChildAstNodes.Single().As<bitwise_expression_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return this.BitwiseExpression.Execute(context, commandRuntime);
        }
    }
}

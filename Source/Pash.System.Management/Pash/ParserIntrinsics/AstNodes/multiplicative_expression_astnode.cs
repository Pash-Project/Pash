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
    public class multiplicative_expression_astnode : _astnode
    {
        public readonly format_expression_astnode FormatExpression;

        public multiplicative_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        multiplicative_expression:
            ////            format_expression
            ////            multiplicative_expression   *   new_lines_opt   format_expression
            ////            multiplicative_expression   /   new_lines_opt   format_expression
            ////            multiplicative_expression   %   new_lines_opt   format_expression

            if (this.ChildAstNodes.Count == 1)
            {
                this.FormatExpression = this.ChildAstNodes.Single().As<format_expression_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return this.FormatExpression.Execute(context, commandRuntime);
        }
    }
}

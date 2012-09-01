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
    public class comparison_expression_astnode : _astnode
    {
        public readonly additive_expression_astnode AdditiveExpression;

        public comparison_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        comparison_expression:
            ////            additive_expression
            ////            comparison_expression   comparison_operator   new_lines_opt   additive_expression

            if (this.ChildAstNodes.Count == 1)
            {
                this.AdditiveExpression = this.ChildAstNodes.Single().Cast<additive_expression_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return this.AdditiveExpression.Execute(context, commandRuntime);
        }
    }
}

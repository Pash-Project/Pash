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
    public class expression_astnode : _astnode
    {
        public readonly logical_expression_astnode LogicalExpression;

        public expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        expression:
            ////            logical_expression
            this.LogicalExpression = this.ChildAstNodes.Single().Cast<logical_expression_astnode>();
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return this.LogicalExpression.Execute(context, commandRuntime);
        }
    }
}

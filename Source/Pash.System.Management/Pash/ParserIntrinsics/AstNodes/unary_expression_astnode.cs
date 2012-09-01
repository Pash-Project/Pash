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
    public class unary_expression_astnode : _astnode
    {
        public readonly primary_expression_astnode PrimaryExpression;
        public readonly expression_with_unary_operator_astnode ExpressionWithUnaryOperator;

        public unary_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        unary_expression:
            ////            primary_expression
            ////            expression_with_unary_operator
            if (this.ChildAstNodes.Single() is primary_expression_astnode)
            {
                this.PrimaryExpression = this.ChildAstNodes.Single().As<primary_expression_astnode>();
            }

            else if (this.ChildAstNodes.Single() is expression_with_unary_operator_astnode)
            {
                this.ExpressionWithUnaryOperator = this.ChildAstNodes.Single().As<expression_with_unary_operator_astnode>();
            }

            else throw new InvalidOperationException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.PrimaryExpression != null)
            {
                return this.PrimaryExpression.Execute(context, commandRuntime);
            }

            if (this.ExpressionWithUnaryOperator != null)
            {
                return this.ExpressionWithUnaryOperator.Execute(context, commandRuntime);
            }

            throw new Exception();
        }
    }
}

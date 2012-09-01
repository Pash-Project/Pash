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
    public class command_name_expr_astnode : _astnode
    {
        public readonly command_name_astnode CommandName;
        public readonly primary_expression_astnode PrimaryExpression;

        public command_name_expr_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command_name_expr:
            ////            command_name
            ////            primary_expression

            if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.command_name)
            {
                this.CommandName = this.ChildAstNodes.Single().As<command_name_astnode>();
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.primary_expression)
            {
                this.PrimaryExpression = this.ChildAstNodes.Single().As<primary_expression_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.CommandName != null)
            {
                return this.CommandName.Name;
            }

            else if (this.PrimaryExpression != null)
            {
                return this.PrimaryExpression.Execute(context, commandRuntime);
            }

            else throw new InvalidOperationException(this.ToString());
        }
    }
}

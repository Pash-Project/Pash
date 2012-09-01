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
    public class primary_expression_astnode : _astnode
    {
        public readonly value_astnode Value;

        public primary_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        primary_expression:
            ////            value
            ////            member_access
            ////            element_access
            ////            invocation_expression
            ////            post_increment_expression
            ////            post_decrement_expression

            if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.value)
            {
                this.Value = this.ChildAstNodes.Single().Cast<value_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.Value != null) return this.Value.Execute(context, commandRuntime);

            throw new NotImplementedException(this.ToString());
        }
    }
}

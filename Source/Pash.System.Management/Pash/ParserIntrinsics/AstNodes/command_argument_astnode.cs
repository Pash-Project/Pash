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

    public class command_argument_astnode : _astnode
    {
        public readonly command_name_expr_astnode CommandNameExpression;

        public command_argument_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command_argument:
            ////            command_name_expr

            if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.command_name_expr)
            {
                this.CommandNameExpression = this.ChildAstNodes.Single().As<command_name_expr_astnode>();
            }
        }
    }
}

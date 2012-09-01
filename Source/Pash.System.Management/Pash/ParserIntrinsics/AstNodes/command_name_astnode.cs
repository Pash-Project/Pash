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
    public class command_name_astnode : _astnode
    {
        public readonly string Name;

        public command_name_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command_name:
            ////            generic_token
            ////            generic_token_with_subexpr

            if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.generic_token)
            {
                this.Name = this.parseTreeNode.ChildNodes.Single().FindTokenAndGetText();
            }

            else throw new NotImplementedException(this.ToString());
        }
    }
}

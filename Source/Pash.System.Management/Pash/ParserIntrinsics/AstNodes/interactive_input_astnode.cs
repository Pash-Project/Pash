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
    public class interactive_input_astnode : _astnode
    {
        public readonly script_block_astnode ScriptBlock;

        public interactive_input_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        interactive_input:
            ////            script_block
            this.ScriptBlock = (script_block_astnode)this.ChildAstNodes.Single();
        }
    }
}

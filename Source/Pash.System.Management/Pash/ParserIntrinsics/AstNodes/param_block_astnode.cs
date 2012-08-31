using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Diagnostics;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class param_block_astnode : _astnode
    {
        public param_block_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }
    }
}

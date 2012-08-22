using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;

namespace Pash.ParserIntrinsics.Nodes
{
    public class command_name_node : _node
    {
        public command_name_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }
    }
}

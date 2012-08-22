using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;

namespace Pash.ParserIntrinsics.Nodes
{
    public class comparison_expression_node : _node
    {
        public comparison_expression_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Pash.ParserIntrinsics.Nodes
{
    public class hexadecimal_integer_literal_node : _node
    {
        public hexadecimal_integer_literal_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            string text = parseTreeNode.FindTokenAndGetText();

            return Convert.ToInt32(text, 16);
        }
    }
}

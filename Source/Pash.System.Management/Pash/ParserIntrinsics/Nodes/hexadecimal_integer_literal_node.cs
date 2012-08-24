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

namespace Pash.ParserIntrinsics.Nodes
{
    public class hexadecimal_integer_literal_node : _node
    {
        public hexadecimal_integer_literal_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue(context));

            // TODO: extract the value to the pipeline
        }

        internal override object GetValue(ExecutionContext context)
        {
            string text = parseTreeNode.FindTokenAndGetText();

            return Convert.ToInt32(text, 16);
        }
    }
}

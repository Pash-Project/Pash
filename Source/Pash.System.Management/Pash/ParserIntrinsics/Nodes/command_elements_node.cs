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
using System.Management.Automation.Runspaces;

namespace Pash.ParserIntrinsics.Nodes
{
    public class command_elements_node : _node
    {
        public command_elements_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return parseTreeNode.ChildNodes
                .Select(node => node.AstNode)
                .Cast<_node>()
                .Select(astNode => astNode.Execute(context, commandRuntime))
                .Select(o => new CommandParameter(null, o))
                ;
        }
    }
}

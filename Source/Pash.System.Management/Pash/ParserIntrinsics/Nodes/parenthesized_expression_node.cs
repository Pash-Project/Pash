using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Pash.ParserIntrinsics.Nodes
{
    public class parenthesized_expression_node : _node
    {
        readonly _node pipelineNode;

        public parenthesized_expression_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            pipelineNode = (_node)parseTreeNode.ChildNodes[1].AstNode;
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
            context.PushPipeline(pipeline);
            Collection<PSObject> results = pipelineNode.Execute(context, commandRuntime) as Collection<PSObject>;
            context.PopPipeline();

            if (results.Count == 0)
                return null;

            if (results.Count == 1)
                return results.Single();

            // TODO: make sure that the array.ToString calls the ToString on each PSObject
            return results.ToArray();
        }
    }
}

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
        readonly pipeline_node pipelineNode;

        public parenthesized_expression_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            pipelineNode = (pipeline_node)parseTreeNode.ChildNodes[1].AstNode;
        }

        internal override object GetValue(ExecutionContext context)
        {
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
            context.PushPipeline(pipeline);
            Collection<PSObject> results = pipelineNode.GetValue(context) as Collection<PSObject>;
            context.PopPipeline();

            if (results.Count == 0)
                return null;

            if (results.Count == 1)
                return results[0];

            // TODO: make sure that the array.ToString calls the ToString on each PSObject

            PSObject[] array = new PSObject[results.Count];
            int index = 0;
            foreach (PSObject psObject in results)
            {
                array[index++] = psObject;
            }

            return array;
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue(context));

            // TODO: extract the value to the pipeline
        }
    }
}

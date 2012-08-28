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

namespace Pash.ParserIntrinsics.AstNodes
{
    public class pipeline_astnode : _astnode
    {
        public pipeline_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.parseTreeNode.ChildNodes.Count == 1)
            {
                return ((_astnode)this.parseTreeNode.ChildNodes.Single().AstNode).Execute(context, commandRuntime);
            }

            if (this.parseTreeNode.ChildNodes.Count == 2)
            {
                // TODO: rewrite this - it should expand the commands in the original pipe

                ExecutionContext subContext = context.CreateNestedContext();
                subContext.inputStreamReader = context.inputStreamReader;

                PipelineCommandRuntime subRuntime = new PipelineCommandRuntime(((PipelineCommandRuntime)commandRuntime).pipelineProcessor);

                var results = ((_astnode)this.parseTreeNode.ChildNodes[0].AstNode).Execute(subContext, subRuntime);

                subContext = context.CreateNestedContext();
                subContext.inputStreamReader = new PSObjectPipelineReader(new[] { results });

                subRuntime = new PipelineCommandRuntime(((PipelineCommandRuntime)commandRuntime).pipelineProcessor);
                return ((_astnode)this.parseTreeNode.ChildNodes[1].AstNode).Execute(subContext, subRuntime);
            }

            throw new NotImplementedException();

        }
    }
}
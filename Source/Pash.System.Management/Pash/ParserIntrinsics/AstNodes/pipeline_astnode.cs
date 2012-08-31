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
        public readonly command_astnode Command;

        public pipeline_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        pipeline:
            ////            assignment_expression
            ////            expression   redirections_opt  pipeline_tail_opt
            ////            command   pipeline_tail_opt

            if (this.parseTreeNode.ChildNodes[0].Term == Grammar.command)
            {
                this.Command = this.ChildAstNodes.Single().As<command_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.Command != null)
            {
                return this.Command.Execute(context, commandRuntime);
            }

            throw new NotImplementedException(this.ToString());
        }

        //        ExecutionContext subContext = context.CreateNestedContext();
        //        subContext.inputStreamReader = context.inputStreamReader;

        //        PipelineCommandRuntime subRuntime = new PipelineCommandRuntime(((PipelineCommandRuntime)commandRuntime).pipelineProcessor);

        //        var results = ChildAstNodes.First().Execute_old(subContext, subRuntime);

        //        subContext = context.CreateNestedContext();
        //        subContext.inputStreamReader = new PSObjectPipelineReader(new[] { results });

        //        subRuntime = new PipelineCommandRuntime(((PipelineCommandRuntime)commandRuntime).pipelineProcessor);
        //        return ChildAstNodes.Skip(1).First().Execute_old(subContext, subRuntime);
    }
}
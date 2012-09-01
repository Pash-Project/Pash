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
        public readonly assignment_expression_astnode AssignmentExpression;
        public readonly expression_astnode Expression;
        public readonly command_astnode Command;

        public pipeline_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        pipeline:
            ////            assignment_expression
            ////            expression   redirections_opt  pipeline_tail_opt
            ////            command   pipeline_tail_opt

            if (this.parseTreeNode.ChildNodes[0].Term == Grammar.assignment_expression)
            {
                this.AssignmentExpression = this.ChildAstNodes.Single().As<assignment_expression_astnode>();
            }

            else if (this.parseTreeNode.ChildNodes[0].Term == Grammar.expression)
            {
                if (this.ChildAstNodes.Count > 1) throw new NotImplementedException(this.ToString());
                this.Expression = this.ChildAstNodes.Single().As<expression_astnode>();
            }

            else if (this.parseTreeNode.ChildNodes[0].Term == Grammar.command)
            {
                if (this.ChildAstNodes.Count > 1) throw new NotImplementedException(this.ToString());

                this.Command = this.ChildAstNodes.Single().As<command_astnode>();
            }

            else throw new InvalidOperationException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.AssignmentExpression != null)
            {
                this.AssignmentExpression.Execute(context, commandRuntime);
                return null;
            }

            if (this.Expression != null)
            {
                return this.Expression.Execute(context, commandRuntime);
            }

            if (this.Command != null)
            {
                return this.Command.Execute(context, commandRuntime);
            }

            throw new InvalidOperationException(this.ToString());
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
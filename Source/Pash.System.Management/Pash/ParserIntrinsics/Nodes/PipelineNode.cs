using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.Nodes
{
    internal class PipelineNode : NonTerminalNode
    {
        public List<ASTNode> Pipeline { get; private set; }

        public static PipelineNode GetPipeline(Parser theParser)
        {
            PipelineNode pipeline = null;

            if (theParser != null)
            {
                object objLeft = theParser.GetReductionSyntaxNode(0);
                object objRight = theParser.GetReductionSyntaxNode(2);

                if (objLeft is PipelineNode)
                {
                    pipeline = (PipelineNode)objLeft;
                    pipeline.AddItem(objRight as ASTNode);
                }
                else if (objRight is PipelineNode)
                {
                    pipeline = (PipelineNode)objRight;
                    pipeline.Insert(0, objLeft as ASTNode);
                }
            }

            if (pipeline == null)
            {
                pipeline = new PipelineNode(theParser);
                if (theParser != null)
                {
                    pipeline.AddItemFromParser(theParser, 0);
                    pipeline.AddItemFromParser(theParser, 2);
                }
            }

            return pipeline;
        }

        private PipelineNode(Parser theParser)
            : base(theParser)
        {
            Pipeline = new List<ASTNode>();
        }

        public void AddItemFromParser(Parser theParser, int index)
        {
            object obj = Token(theParser, index);

            if (obj is ASTNode)
                Pipeline.Add(obj as ASTNode);
        }

        public void AddItem(ASTNode item)
        {
            Pipeline.Add(item);
        }

        public void Insert(int index, ASTNode item)
        {
            Pipeline.Insert(0, item);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Pipeline.Count; i++)
            {
                ASTNode node = Pipeline[i];
                sb.Append(node.ToString());

                if (i + 1 != Pipeline.Count)
                    sb.Append(" | ");
            }
            return sb.ToString();
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            // TODO: rewrite this - it should expand the commands in the original pipe

            PipelineCommandRuntime subRuntime = null;

            foreach (ASTNode node in Pipeline)
            {
                ExecutionContext subContext = context.CreateNestedContext();

                if (subRuntime == null)
                {
                    subContext.inputStreamReader = context.inputStreamReader;
                }
                else
                {
                    subContext.inputStreamReader = new PSObjectPipelineReader(subRuntime.outputResults);
                }

                subRuntime = new PipelineCommandRuntime(((PipelineCommandRuntime)commandRuntime).pipelineProcessor);
                subContext.inputStreamReader = subContext.inputStreamReader;

                node.Execute(subContext, subRuntime);
            }
        }
    }
}

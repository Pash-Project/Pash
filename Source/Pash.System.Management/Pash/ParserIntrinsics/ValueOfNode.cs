using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    internal class ValueOfNode : NonTerminalNode
    {
        private ASTNode nodeValue;

        public ValueOfNode(Parser parser) : base(parser)
        {
            nodeValue = Node(parser, 1);
        }

        internal override object GetValue(ExecutionContext context)
        {
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
            context.PushPipeline(pipeline);
            Collection<PSObject> results = nodeValue.GetValue(context) as Collection<PSObject>;
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

        public override string ToString()
        {
            return nodeValue.ToString();
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue(context));

            // TODO: extract the value to the pipeline
        }
    }
}
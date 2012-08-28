using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.Nodes
{
    internal class AssignmentNode : NonTerminalNode
    {
        private ASTNode lValue;
        private ASTNode rValue;

        public AssignmentNode(Parser theParser)
            : base(theParser)
        {
            lValue = Node(theParser, 0);
            rValue = Node(theParser, 2);
        }

        public override string ToString()
        {
            return lValue.ToString() + " = " + rValue.ToString();
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ExecutionContext nestedContext = context.CreateNestedContext();

            if (lValue is VariableNode)
            {
                VariableNode varNode = (VariableNode)lValue;

                if (!(context.CurrentRunspace is LocalRunspace))
                    throw new InvalidOperationException("Invalid context");

                // MUST: fix this with the commandRuntime
                Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
                context.PushPipeline(pipeline);

                try
                {
                    Command cmd = new Command("Set-Variable");
                    cmd.Parameters.Add("Name", new string[] { varNode.Text });
                    cmd.Parameters.Add("Value", rValue.GetValue(context));
                    // TODO: implement command invoke
                    pipeline.Commands.Add(cmd);
                    pipeline.Invoke();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    context.PopPipeline();
                }
            }
        }
    }
}

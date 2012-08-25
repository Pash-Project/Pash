using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;

namespace Pash.ParserIntrinsics.Nodes
{
    public class command_node : _node
    {
        private Collection<PSObject> _results;

        public command_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            var commandText = parseTreeNode.ChildNodes[0].FindTokenAndGetText();
            CommandInfo commandInfo = ((LocalRunspace)context.CurrentRunspace).CommandManager.FindCommand(commandText);

            if (commandInfo == null)
                throw new InvalidOperationException(commandText);

            IEnumerable<CommandParameter> parameters = new CommandParameter[] { };
            if (parseTreeNode.ChildNodes.Count == 2)
            {
                var commandElementsNode = (command_elements_node)parseTreeNode.ChildNodes[1].AstNode;
                parameters = (IEnumerable<CommandParameter>)commandElementsNode.Execute(context, commandRuntime);
            }

            // MUST: fix this with the commandRuntime
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();

            // TODO: Fill the pipeline with input data?
            //pipeline.Input.Write(context.inputStreamReader);

            context.PushPipeline(pipeline);

            try
            {
                var command = new Command(commandText);
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                pipeline.Commands.Add(command);
                return pipeline.Invoke();
            }
            finally
            {
                context.PopPipeline();
            }

        }
    }
}

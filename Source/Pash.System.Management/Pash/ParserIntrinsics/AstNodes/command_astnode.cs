using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Extensions.String;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class command_astnode : _astnode
    {
        public readonly string Name;
        public readonly IEnumerable<command_element_astnode> CommandElements;

        public command_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command:
            ////            command_name   command_elements_opt
            ////            command_invocation_operator   command_module_opt  command_name_expr   command_elements_opt

            if (this.parseTreeNode.ChildNodes[0].Term == Grammar.command_name)
            {
                this.Name = this.ChildAstNodes[0].As<command_name_astnode>().Name;

                this.CommandElements = this.ChildAstNodes.Skip(1).Cast<command_element_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            CommandInfo commandInfo = ((LocalRunspace)context.CurrentRunspace).CommandManager.FindCommand(Name);

            if (commandInfo == null)
                throw new InvalidOperationException(Name);

            IEnumerable<CommandParameter> parameters = new CommandParameter[] { };
            //if (parseTreeNode.ChildNodes.Count == 2)
            //{
            //    var commandElementsAstNode = ChildAstNodes.Skip(1).First().As<command_elements_astnode>();
            //    parameters = (IEnumerable<CommandParameter>)commandElementsAstNode.Execute_old(context, commandRuntime);
            //}

            // MUST: fix this with the commandRuntime
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();

            pipeline.Input.Write(context.inputStreamReader.ReadToEnd(), true);


            var command = new Command(Name);

            for (int i = 0; i < this.CommandElements.Count(); i++)
            {
                var commandElement = this.CommandElements.ElementAt(i);

                if (commandElement.Argument != null)
                {
                    command.Parameters.Add(new CommandParameter(null, commandElement.Argument.CommandNameExpression.Execute(context, commandRuntime)));
                }

                else if (commandElement.Parameter != null)
                {
                    // TODO: '-Foo:bar' and '-Foo bar'
                    command.Parameters.Add(new CommandParameter(commandElement.Parameter.Name, true));
                }
            }

            pipeline.Commands.Add(command);
            context.PushPipeline(pipeline);
            try
            {
                return pipeline.Invoke();
            }
            finally
            {
                context.PopPipeline();
            }

        }
    }
}

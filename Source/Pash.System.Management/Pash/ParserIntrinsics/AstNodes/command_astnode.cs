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
        public readonly command_elements_astnode CommandElements;

        public command_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command:
            ////            command_name   command_elements_opt
            ////            command_invocation_operator   command_module_opt  command_name_expr   command_elements_opt

            if (this.parseTreeNode.ChildNodes[0].Term == Grammar.command_name)
            {
                this.Name = this.ChildAstNodes[0].As<command_name_astnode>().Name;

                if (this.parseTreeNode.ChildNodes.Count == 2)
                {
                    this.CommandElements = this.ChildAstNodes[1].As<command_elements_astnode>();
                }
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

            if (this.CommandElements != null)
            {
                for (int i = 0; i < this.CommandElements.Items.Count(); i++)
                {
                    var commandElement = this.CommandElements.Items.ElementAt(i);

                    if (commandElement.Argument != null)
                    {
                        command.Parameters.Add(new CommandParameter(null, commandElement.Argument.CommandNameExpression.Execute(context, commandRuntime)));
                    }

                    else if (commandElement.Parameter != null)
                    {
                        string name = commandElement.Parameter.Name;
                        object value;

                        // TODO: '-Foo bar', where 'bar' is an argument to '-Foo', e.g. 'Get-ChildItem -Path C:\'
                        if (commandElement.Parameter.Colon)
                        {
                            i++;
                            if (i == this.CommandElements.Items.Count()) throw new Exception("Parameter '{0}' requires an argument.".FormatString(commandElement.Parameter.Name));

                            value = this.CommandElements.Items.ElementAt(i);
                        }
                        else
                        {
                            // TODO: what if this is not a switch? Postpone this code until binding time.
                            value = true;
                        }

                        command.Parameters.Add(new CommandParameter(name, value));
                    }
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

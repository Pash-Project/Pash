using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Extensions.String;
using System.Management.Automation.Runspaces;

namespace Pash.ParserIntrinsics.Nodes
{
    public class variable_node : _node
    {
        public variable_node(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////        variable:
            ////            $$
            ////            $?
            ////            $^
            ////            $   variable_scope_opt  variable_characters
            ////            @   variable_scope_opt   variable_characters
            ////            braced_variable

            string name = GetVariableName(Text);

            return GetVariable(context, name);
        }

        internal static string GetVariableName(string variableText)
        {
            var matches = Regex.Match(variableText, PowerShellGrammar.Terminals.variable.Pattern);

            if (matches.Groups[PowerShellGrammar.Terminals._dollar_dollar.Name].Success)
            {
                return "$";
            }
            else if (matches.Groups[PowerShellGrammar.Terminals._dollar_question.Name].Success)
            {
                return "?";
            }
            else if (matches.Groups[PowerShellGrammar.Terminals._dollar_hat.Name].Success)
            {
                return "_";
            }
            else if (matches.Groups[PowerShellGrammar.Terminals._ordinary_variable.Name].Success)
            {
                return matches.Groups[PowerShellGrammar.Terminals.variable_characters.Name].Value;
            }
            else if (matches.Groups[PowerShellGrammar.Terminals._splatted_variable.Name].Success)
            {
                throw new NotImplementedException(variableText);
            }
            else if (matches.Groups[PowerShellGrammar.Terminals.braced_variable.Name].Success)
            {
                return matches.Groups[PowerShellGrammar.Terminals.braced_variable_characters.Name].Value;
            }
            else throw new NotImplementedException(variableText);
        }

        object GetVariable(ExecutionContext context, string name)
        {
            // Should we do this instead?
            //context.GetVariable(name)
            ExecutionContext nestedContext = context.CreateNestedContext();

            if (!(context.CurrentRunspace is LocalRunspace))
                throw new InvalidOperationException("Invalid context");

            // MUST: fix this with the commandRuntime
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
            context.PushPipeline(pipeline);

            try
            {
                Command cmd = new Command("Get-Variable");
                cmd.Parameters.Add("Name", new string[] { name });
                // TODO: implement command invoke
                pipeline.Commands.Add(cmd);

                return pipeline.Invoke().First();
            }
            finally
            {
                context.PopPipeline();
            }

        }
    }
}

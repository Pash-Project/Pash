using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    internal class VariableNode : TerminalNode
    {
        public VariableNode(Parser theParser)
            : base(theParser)
        {
            // strip the $ (dollar) sign

            if (string.IsNullOrEmpty(Text))
                return;

            if (Text[0] == '$')
                Text = Text.Substring(1);
        }

        public override string ToString()
        {
            return "$" + Text;
        }

        internal override void Execute(Pash.Implementation.ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ExecutionContext nestedContext = context.CreateNestedContext();

            if (! (context.CurrentRunspace is LocalRunspace))
                throw new InvalidOperationException("Invalid context");

            // MUST: fix this with the commandRuntime
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
            context.PushPipeline(pipeline);

            try
            {
                Command cmd = new Command("Get-Variable");
                cmd.Parameters.Add("Name", new string[] { Text });
                // TODO: implement command invoke
                pipeline.Commands.Add(cmd);

                commandRuntime.WriteObject(pipeline.Invoke(), true);
                //context.outputStreamWriter.Write(pipeline.Invoke(), true);
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

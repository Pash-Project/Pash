using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    internal class CmdletNode : NonTerminalNode
    {
        public string CmdletName { get; private set; }
        public ParamsListNode Params { get; private set; }
        private Collection<PSObject> _results;

        public CmdletNode(Parser theParser)
            : base(theParser)
        {
            if (Token(theParser, 0) is AnyWordNode)
                CmdletName = ((AnyWordNode)Token(theParser, 0)).Text;

            Params = ParamsListNode.GetParamsListFromRight(theParser);
        }

        public CmdletNode(Parser theParser, AnyWordNode anyWord)
            : base(theParser)
        {
            CmdletName = anyWord.Text;

            Params = ParamsListNode.GetParamsList(theParser);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CmdletName);
            if (Params.Params.Count > 0)
            {
                sb.Append(' ');
                sb.Append(Params.ToString());
            }
            return sb.ToString();
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (!(context.CurrentRunspace is LocalRunspace))
                throw new InvalidOperationException(string.Format("Command \"{0}\" was not found.", CmdletName));

            CommandManager cmdMgr = ((LocalRunspace)context.CurrentRunspace).CommandManager;

            CommandInfo cmdInfo = cmdMgr.FindCommand(CmdletName);

            if (cmdInfo == null)
                throw new InvalidOperationException(string.Format("Command \"{0}\" was not found.", CmdletName));

            // MUST: fix this with the commandRuntime
            Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();
            context.PushPipeline(pipeline);

            try
            {
                // TODO: implement command invoke

                Command command = new Command(CmdletName);
                foreach (string param in Params.Params)
                {
                    command.Parameters.Add(null, param);
                }
                pipeline.Commands.Add(command);
                _results = pipeline.Invoke();

                commandRuntime.WriteObject(_results, true);
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

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    internal class AnyWordNode : TerminalNode
    {
        private bool _bExecuted;
        private Collection<PSObject> _results;

        public AnyWordNode(Parser theParser)
            : base(theParser)
        {
            _bExecuted = false;
            _results = new Collection<PSObject>();
        }

        internal override object GetValue(ExecutionContext context)
        {
            if (! _bExecuted)
            {
                if (! (context.CurrentRunspace is LocalRunspace))
                    throw new InvalidOperationException(string.Format("Command \"{0}\" was not found.", Text));

                CommandManager cmdMgr = ((LocalRunspace) context.CurrentRunspace).CommandManager;

                CommandInfo cmdInfo = cmdMgr.FindCommand(Text);

                if (cmdInfo == null)
                    throw new InvalidOperationException(string.Format("Command \"{0}\" was not found.", Text));

                // MUST: fix this with the commandRuntime
                Pipeline pipeline = context.CurrentRunspace.CreateNestedPipeline();

                // Fill the pipeline with input data
                pipeline.Input.Write(context.inputStreamReader);
                context.PushPipeline(pipeline);

                try
                {
                    // TODO: implement command invoke
                    pipeline.Commands.Add(Text);
                    _results = pipeline.Invoke();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    context.PopPipeline();
                }

                _bExecuted = true;
            }

            return _results;
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue(context), true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Management.Automation;
using Pash.ParserIntrinsics;

namespace Pash.Implementation
{
    internal class ScriptProcessor : CommandProcessorBase
    {
        private static object SyncRoot = new object();
        private static PashParser _parser;

        private PashParser Parser
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                if (_parser == null)
                {
                    lock (SyncRoot)
                    {
                        if (_parser == null)
                        {
                            _parser = PashParser.Initialize();
                        }
                    }
                }

                return _parser;
            }
        }

        public ScriptProcessor(ScriptInfo scriptInfo)
            : base(scriptInfo)
        {
        }

        internal override ICommandRuntime CommandRuntime { get; set; }

        internal override void BindArguments(PSObject obj)
        {
            // TODO: bind arguments to "param" statement
        }

        internal override void Initialize()
        {
            // TODO: initialize ScriptProcessor
        }

        internal override void ProcessRecord()
        {
            //Parser.Parse(this.CommandInfo.Definition, 

            //PashParser parser = new PashParser(pFactory);

            if (string.IsNullOrEmpty(CommandInfo.Definition))
            {
                // TODO: what is the behavior of Posh when the script is empty?
                return;
            }

            Parser.Parse(CommandInfo.Definition);

            if (Parser.ErrorString != null)
            {
                // TODO: implement a parsing exception
                throw new Exception(Parser.ErrorString);
            }

            // TODO: if a tree is empty?
            if (Parser.SyntaxTree == null)
                return;

            ExecutionContext context = ExecutionContext.Clone();
            //PipelineCommandRuntime runtime = (PipelineCommandRuntime) CommandRuntime;
            //context.outputStreamWriter = new ObjectStreamWriter(runtime.outputResults);

            Parser.SyntaxTree.Execute(context, CommandRuntime);
        }

        internal override void Complete()
        {
            // TODO: do a full cleanup
        }
    }
}

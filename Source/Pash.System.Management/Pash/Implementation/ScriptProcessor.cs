using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Management.Automation;
using Pash.ParserIntrinsics;
using Irony.Parsing;
using Extensions.String;
using Pash.ParserIntrinsics.AstNodes;

namespace Pash.Implementation
{
    internal class ScriptProcessor : CommandProcessorBase
    {
        private static object SyncRoot = new object();
        private static Parser _parser;
        private static PowerShellGrammar _grammar;

        private Parser Parser
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
                            _grammar = new PowerShellGrammar.InteractiveInput();
                            _parser = new Parser(_grammar);
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

            var results = Parser.Parse(CommandInfo.Definition);

            if (results.HasErrors)
            {
                // TODO: implement a parsing exception
                throw new Exception(results.ParserMessages.JoinString("\n"));
            }

            // TODO: if a tree is empty?
            if (results.Root == null) throw new Exception();

            ExecutionContext context = ExecutionContext.Clone();
            //PipelineCommandRuntime runtime = (PipelineCommandRuntime) CommandRuntime;
            //context.outputStreamWriter = new ObjectStreamWriter(runtime.outputResults);

            CommandRuntime.WriteObject(((_astnode)results.Root.AstNode).Execute(context, CommandRuntime), true);
        }

        internal override void Complete()
        {
            // TODO: do a full cleanup
        }
    }
}

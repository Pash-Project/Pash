// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using Pash.Implementation;
using Pash.ParserIntrinsics;
using System.Management.Pash.Implementation;

namespace System.Management.Automation
{
    /// <summary>
    /// Parser instristics about executing a command.
    /// </summary>
    public class CommandInvocationIntrinsics
    {
        private ExecutionContext executionContext;
        private PipelineCommandRuntime commandRuntime;

        internal CommandInvocationIntrinsics(ExecutionContext executionContext, PipelineCommandRuntime commandRuntime)
        {
            this.executionContext = executionContext;
            this.commandRuntime = commandRuntime;
        }

        public string ExpandString(string source) { throw new NotImplementedException(); }

        public Collection<PSObject> InvokeScript(string script)
        {
            return InvokeScript(script, false, PipelineResultTypes.None, null);
        }

        public Collection<PSObject> InvokeScript(string script, bool useNewScope, PipelineResultTypes writeToPipeline, IList input, params object[] args)
        {
            var context = useNewScope ? executionContext.Clone(ScopeUsages.NewScriptScope)
                                      : executionContext;
            //Let's see on the long run if there is an easier solution for this #ExecutionContextChange
            //we need to change the global execution context to change the scope we are currently operating in
            executionContext.CurrentRunspace.ExecutionContext = context;
            try
            {
                ScriptBlock scriptBlock = NewScriptBlock(script);
                var executionVisitor = new ExecutionVisitor(context, commandRuntime);
                // sburnicki - handle ExitException
                scriptBlock.Ast.Visit(executionVisitor);
            }
            finally //make sure we set back the old execution context, no matter what happened
            {
                executionContext.CurrentRunspace.ExecutionContext = executionContext;
            }
            return new Collection<PSObject>();
        }
        
        public ScriptBlock NewScriptBlock(string scriptText)
        {
            return new ScriptBlock(CommandManager.ParseInput(scriptText));
        }
    }
}

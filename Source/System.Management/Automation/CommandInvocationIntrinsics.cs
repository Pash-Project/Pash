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
            ScriptBlock scriptBlock = NewScriptBlock(script);
            var executionVisitor = new ExecutionVisitor(executionContext, commandRuntime);
            scriptBlock.Ast.Visit(executionVisitor);

            return new Collection<PSObject>();
        }
        
        public ScriptBlock NewScriptBlock(string scriptText)
        {
            return new ScriptBlock(PowerShellGrammar.ParseInteractiveInput(scriptText));
        }
    }
}

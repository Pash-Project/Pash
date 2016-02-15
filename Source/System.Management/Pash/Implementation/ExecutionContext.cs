// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections;

namespace Pash.Implementation
{
    internal class ExecutionContext
    {
        internal RunspaceConfiguration RunspaceConfiguration { get; private set; }
        internal ObjectStream InputStream { get; set; }
        internal ObjectStream OutputStream { get; set; }
        internal ObjectStream ErrorStream { get; set; }
        internal Runspace CurrentRunspace { get; set; }
        internal Stack<Pipeline> _pipelineStack;
        internal PSHost LocalHost { get; set; }
        internal SessionState SessionState { get; private set; }
        internal SessionStateGlobal SessionStateGlobal { get; private set; }

        private ExecutionContext()
        {
            // TODO: initialize all the default settings
            _pipelineStack = new Stack<Pipeline>();
        }

        public ExecutionContext(PSHost host, RunspaceConfiguration config)
            : this()
        {
            RunspaceConfiguration = config;
            LocalHost = host;
            SessionStateGlobal = new SessionStateGlobal(this);
            SessionState = new SessionState(SessionStateGlobal);
        }

        public ExecutionContext Clone(ScopeUsages scopeUsage = ScopeUsages.CurrentScope)
        {
            return Clone(SessionState, scopeUsage);
        }

        public ExecutionContext Clone(SessionState sessionState, ScopeUsages scopeUsage)
        {
            var sstate = (scopeUsage == ScopeUsages.CurrentScope) ? sessionState : new SessionState(sessionState);
            if (scopeUsage == ScopeUsages.NewScriptScope)
            {
                sstate.IsScriptScope = true;
            }
            var context = new ExecutionContext
            {
                InputStream = InputStream,
                OutputStream = OutputStream,
                ErrorStream = ErrorStream,
                CurrentRunspace = CurrentRunspace,
                LocalHost = LocalHost,
                WriteSideEffectsToPipeline = WriteSideEffectsToPipeline,
                SessionStateGlobal = SessionStateGlobal,
                SessionState = sstate
            };

            // TODO: copy (not reference) all the variables to allow nested context <- what does it mean?

            return context;
        }

        public ExecutionContext CreateNestedContext()
        {
            // I guess the idea of this function is that Input/Error/Ouput streams aren't copied
            // However, as it works as it is, we won't change this, yet
            ExecutionContext nestedContext = Clone();

            //nestedContext.

            return nestedContext;
        }

        internal void PushPipeline(Pipeline pipeline)
        {
            // TODO: make sure that the "CurrentPipeline" is in the stack

            _pipelineStack.Push(pipeline);

            // TODO: create a new pipeline and replace the current one with it
            if (pipeline is LocalPipeline)
            {
                ((LocalPipeline)pipeline).RerouteExecutionContext(this);
            }

        }

        internal Pipeline PopPipeline()
        {
            Pipeline pipeline = _pipelineStack.Pop();

            // TODO: replace all the streams to point to this new pipeline
            if (pipeline is LocalPipeline)
            {
                ((LocalPipeline)pipeline).RerouteExecutionContext(this);
            }
            return pipeline;
        }

        internal PSVariable GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            return this.SessionState.PSVariable.Get(name);
        }

        internal object GetVariableValue(string name)
        {
            return ((PSVariable)GetVariable(name)).GetBaseObjectValue();
        }

        internal void SetVariable(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            this.SessionState.PSVariable.Set(name, value);
        }

        internal bool WriteSideEffectsToPipeline { get; set; }

        internal void AddToErrorVariable(object error)
        {
            var errorRecordsVar = GetVariable("Error");
            var records = errorRecordsVar.Value as ArrayList;
            if (records == null)
            {
                // TODO: this should never happen as the variable is const. but anyway
                return;
            }
            // make sure it's not added multiple times (e.g. *same* exception thrown through multiple nested pipelines)
            if (!records.Contains(error))
            {
                records.Insert(0, error);
            }
        }

        internal void SetLastExitCodeVariable(int exitCode)
        {
            SetVariable("global:LASTEXITCODE", exitCode);
        }

        internal void SetSuccessVariable(bool success)
        {
            PSVariable questionMarkVariable = SessionState.PSVariable.GetAtScope("?", "global");
            if (questionMarkVariable != null)
            {
                questionMarkVariable.Value = success;
            }
        }
    }
}

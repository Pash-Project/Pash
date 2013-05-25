﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace Pash.Implementation
{
    internal class ExecutionContext
    {
        internal RunspaceConfiguration RunspaceConfiguration { get; private set; }
        internal PipelineReader<PSObject> inputStreamReader { get; set; }
        internal PipelineWriter outputStreamWriter { get; set; }
        internal PipelineWriter errorStreamWriter { get; set; }
        internal Runspace CurrentRunspace { get; set; }
        internal Stack<Pipeline> _pipelineStack;
        internal PSHost LocalHost { get; set; }
        internal Dictionary<string, PSVariable> _variables;
        internal SessionState SessionState { get; private set; }
        internal static SessionStateGlobal _sessionStateGlobal;

        private ExecutionContext()
        {
            // TODO: create a "Global Session state"
            if (_sessionStateGlobal == null)
                _sessionStateGlobal = new SessionStateGlobal(this);

            // TODO: initialize all the default settings
            _pipelineStack = new Stack<Pipeline>();
            _variables = new Dictionary<string, PSVariable>(StringComparer.CurrentCultureIgnoreCase);
            SessionState = new SessionState(_sessionStateGlobal);
        }

        public ExecutionContext(PSHost host, RunspaceConfiguration config)
            : this()
        {
            RunspaceConfiguration = config;
            LocalHost = host;
        }

        public ExecutionContext Clone()
        {
            var context = new ExecutionContext
            {
                inputStreamReader = inputStreamReader,
                outputStreamWriter = outputStreamWriter,
                errorStreamWriter = errorStreamWriter,
                CurrentRunspace = CurrentRunspace,
                LocalHost = LocalHost,
                WriteSideEffectsToPipeline = WriteSideEffectsToPipeline
            };

            // TODO: copy (not reference) all the variables to allow nested context

            return context;
        }

        public ExecutionContext CreateNestedContext()
        {
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

        internal object GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            return this.SessionState.PSVariable.Get(name);
        }

        internal void SetVariable(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            this.SessionState.PSVariable.Set(name, value);
        }

        internal bool WriteSideEffectsToPipeline { get; set; }
    }
}

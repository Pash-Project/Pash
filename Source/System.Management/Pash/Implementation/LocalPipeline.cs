// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections;
using System.Management.Automation;
using Pash.ParserIntrinsics;
using System.Management.Pash.Implementation;

namespace Pash.Implementation
{
    internal class LocalPipeline : Pipeline
    {
        private ObjectStream _inputStream;
        private ObjectStream _outputStream;
        private ObjectStream _errorStream;
        private ObjectStreamWriter _inputPipelineWriter;
        private PSObjectPipelineReader _outputPipelineReader;
        private ObjectPipelineReader _errorPipelineReader;
        private LocalRunspace _runspace;
        private PipelineStateInfo _pipelineStateInfo;
        private bool _isNested;

        public LocalPipeline(LocalRunspace runspace, string command, bool isNested)
            : base()
        {
            _runspace = runspace;
            _inputStream = new ObjectStream(this);
            _outputStream = new ObjectStream(this);
            _errorStream = new ObjectStream(this);
            _inputPipelineWriter = new ObjectStreamWriter(_inputStream);
            _outputPipelineReader = new PSObjectPipelineReader(_outputStream);
            _errorPipelineReader = new ObjectPipelineReader(_errorStream);
            _pipelineStateInfo = new PipelineStateInfo(PipelineState.NotStarted);
            _isNested = isNested;

            if (!string.IsNullOrEmpty(command))
                Commands.AddScript(command, false);
        }

        protected override void Dispose(bool disposing)
        {
            // TODO: implement LocalPipeline.Dispose
        }

        public override PipelineReader<object> Error
        {
            get
            {
                return _errorPipelineReader;
            }
        }

        public override PipelineWriter Input
        {
            get { return _inputPipelineWriter; }
        }

        public override bool IsNested {
            get
            {
                return _isNested;
            }
        }

        public override PipelineReader<PSObject> Output
        {
            get
            {
                return _outputPipelineReader;
            }
        }

        public override PipelineStateInfo PipelineStateInfo
        {
            get
            {
                return _pipelineStateInfo;
            }
        }

        private void SetPipelineState(PipelineState state)
        {
            SetPipelineState(state, null);
        }

        private void SetPipelineState(PipelineState state, Exception reason)
        {
            _pipelineStateInfo = new PipelineStateInfo(state, reason);

            if (StateChanged != null)
                StateChanged(this, new PipelineStateEventArgs(_pipelineStateInfo));
        }

        public override Runspace Runspace
        {
            get
            {
                return _runspace;
            }
        }

        public override event EventHandler<PipelineStateEventArgs> StateChanged;

        public override Pipeline Copy()
        {
            throw new NotImplementedException();
        }

        public override Collection<PSObject> Invoke(IEnumerable input)
        {
            // TODO: run the pipeline on another thread and wait for the completion
            // TODO: nested pipelines: make sure we are running in the same thread as another pipeline, because
            // nested pipelines aren't allowed to run in their own thread. This ensures that there is a "parent" pipeline

            Input.Write(input, true);

            // in a pipeline, the first command enters *always* the ProcessRecord phase, the following commands only
            // if the previous command generated output. To make sure the first command enters that phase, add null
            // if nothing else is in the input stream
            if (_inputStream.Count == 0)
            {
                Input.Write(null);
            }

            string errorId = "BuildingPipelineProcessorFailed";

            ExecutionContext context = _runspace.ExecutionContext.Clone();
            RerouteExecutionContext(context);
            try
            {
                if (!_pipelineStateInfo.State.Equals(PipelineState.NotStarted))
                {
                    throw new InvalidPipelineStateException("Pipeline cannot be started",
                        _pipelineStateInfo.State, PipelineState.NotStarted);
                }

                var pipelineProcessor = BuildPipelineProcessor(context);

                _runspace.AddRunningPipeline(this);
                SetPipelineState(PipelineState.Running);

                errorId = "TerminatingError";
                pipelineProcessor.Execute(context);
                SetPipelineState(PipelineState.Completed);
            }
            catch (ExitException ex)
            {
                // Toplevel pipeline
                if (!IsNested)
                {
                    _runspace.PSHost.SetShouldExit(ex.ExitCode);
                }
                else
                {
                    // nested pipelines propagate the exit command
                    throw;
                }
            }
            catch (Exception ex)
            {
                // in case of throw statement, parse error, or "ThrowTerminatingError"
                // just add to error variable and rethrow that thing
                var errorRecord = (ex is IContainsErrorRecord) ?
                    ((IContainsErrorRecord) ex).ErrorRecord : new ErrorRecord(ex, errorId, ErrorCategory.InvalidOperation, null);
                context.AddToErrorVariable(errorRecord);
                context.SetVariable("global:?", false); // last command was definitely not successfull
                throw;
            }
            _runspace.RemoveRunningPipeline(this);
            return Output.NonBlockingRead();
        }

        PipelineProcessor BuildPipelineProcessor(ExecutionContext context)
        {
            PipelineProcessor pipelineProcessor = new PipelineProcessor();

            // TODO: implement script execution
            foreach (Command command in Commands)
            {
                CommandProcessorBase commandProcessor;

                commandProcessor = command.CreateCommandProcessor(context, _runspace.CommandManager, false);
                pipelineProcessor.Add(commandProcessor);
            }

            return pipelineProcessor;
        }

        internal void RerouteExecutionContext(ExecutionContext context)
        {
            // Filling the input stream with the initial data
            context.InputStream = _inputStream;
            context.OutputStream = _outputStream;
            context.ErrorStream = _errorStream;
        }

        public override void InvokeAsync()
        {
            // TODO: on implementation, check if IsNested is set and throw an error then!
            // nested pipelines must not be invoked asynchronously
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            // TODO: stop pipleine
        }

        public override void StopAsync()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Commands.JoinString(" | ");
        }
    }
}

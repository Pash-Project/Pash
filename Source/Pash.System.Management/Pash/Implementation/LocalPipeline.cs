using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections;
using System.Management.Automation;

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

        public LocalPipeline(LocalRunspace runspace, string command)
            : base()
        {
            _runspace = runspace;
            _inputStream = new ObjectStream();
            _outputStream = new ObjectStream();
            _errorStream = new ObjectStream();
            _inputPipelineWriter = new ObjectStreamWriter(_inputStream);
            _outputPipelineReader = new PSObjectPipelineReader(_outputStream);
            _errorPipelineReader = new ObjectPipelineReader(_errorStream);
            _pipelineStateInfo = new PipelineStateInfo(PipelineState.NotStarted);

            if (! string.IsNullOrEmpty(command))
                Commands.Add(command);
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

        public override bool IsNested
        {
            get { throw new NotImplementedException(); }
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

            Input.Write(input, true);

            SetPipelineState(PipelineState.NotStarted);

            ExecutionContext context = _runspace.ExecutionContext.Clone();
            RerouteExecutionContext(context);

            PipelineProcessor processor = new PipelineProcessor();

            // TODO: implement script execution
            foreach (Command command in Commands)
            {
                if (string.IsNullOrEmpty(command.CommandText))
                    continue;

                CommandProcessorBase commandProcessor = command.CreateCommandProcessor(context, _runspace.CommandManager, false);
                commandProcessor.Initialize();
                processor.Add(commandProcessor);

            }

            // TODO: add a default out-command to the pipeline
            // TODO: it should do the "foreach read from pipe and out via formatter"

            SetPipelineState(PipelineState.Running);
            try
            {
                processor.Execute(context);
                SetPipelineState(PipelineState.Completed);
            }
            catch(Exception ex)
            {
                SetPipelineState(PipelineState.Failed, ex);

                ((LocalRunspace)_runspace).PSHost.UI.WriteErrorLine(ex.Message);
            }

            // TODO: process Error results

            return Output.NonBlockingRead();
        }

        internal void RerouteExecutionContext(ExecutionContext context)
        {
            // Filling the input stream with the initial data
            context.inputStreamReader = new PSObjectPipelineReader(_inputStream);
            context.outputStreamWriter = new ObjectStreamWriter(_outputStream);
            context.errorStreamWriter = new ObjectStreamWriter(_errorStream);
        }

        public override void InvokeAsync()
        {
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
    }
}

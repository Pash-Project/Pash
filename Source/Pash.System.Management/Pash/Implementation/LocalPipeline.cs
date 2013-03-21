// Copyright (C) Pash Contributors. All Rights Reserved. See https://github.com/Pash-Project/Pash/

#region BSD License
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, (either expressed or implied, of the FreeBSD Project.
#endregion

#region GPL License
// This file is part of Pash.
//
// Pash is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Pash is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along
// with Pash.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections;
using System.Management.Automation;
using Pash.ParserIntrinsics;

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

            if (!string.IsNullOrEmpty(command))
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

            PipelineProcessor pipelineProcessor = BuildPipelineProcessor(context);
            if (pipelineProcessor == null) return null;

            // TODO: add a default out-command to the pipeline
            // TODO: it should do the "foreach read from pipe and out via formatter"

            SetPipelineState(PipelineState.Running);
            try
            {
                pipelineProcessor.Execute(context);
                SetPipelineState(PipelineState.Completed);
            }
            catch (Exception ex)
            {
                SetPipelineState(PipelineState.Failed, ex);

                ((LocalRunspace)_runspace).PSHost.UI.WriteErrorLine(ex.Message);
            }

            // TODO: process Error results

            return Output.NonBlockingRead();
        }

        PipelineProcessor BuildPipelineProcessor(ExecutionContext context)
        {
            PipelineProcessor pipelineProcessor = new PipelineProcessor();

            // TODO: implement script execution
            foreach (Command command in Commands)
            {
                CommandProcessorBase commandProcessor;

                try
                {
                    commandProcessor = command.CreateCommandProcessor(context, _runspace.CommandManager, false);
                }
                catch (PowerShellGrammar.ParseException exception)
                {
                    _runspace.PSHost.UI.WriteErrorLine("parse error at " + exception.LogMessage.Location.ToUiString());
                    return null;
                }
                catch (Exception exception)
                {
                    _runspace.PSHost.UI.WriteErrorLine(exception.Message);
                    return null;
                }


                commandProcessor.Initialize();
                pipelineProcessor.Add(commandProcessor);
            }

            return pipelineProcessor;
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

        public override string ToString()
        {
            return string.Join(
                " | ",
                this.Commands
                );
        }
    }
}

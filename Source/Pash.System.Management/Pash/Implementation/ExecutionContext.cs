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
using System.Management.Automation.Host;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;

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
            ExecutionContext context = new ExecutionContext();
            context.inputStreamReader = inputStreamReader;
            context.outputStreamWriter = outputStreamWriter;
            context.errorStreamWriter = errorStreamWriter;
            context.CurrentRunspace = CurrentRunspace;
            context.LocalHost = this.LocalHost;

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

            throw new NotImplementedException();
        }

        internal void SetVariable(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            throw new NotImplementedException();
        }

    }
}

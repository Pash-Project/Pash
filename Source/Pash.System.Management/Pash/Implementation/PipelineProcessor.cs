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
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;
using System.Text;

namespace Pash.Implementation
{
    internal class PipelineProcessor
    {
        // TODO: implement pipeline stopping mechanism
        // Not really supposed to be `const`, that's just to clarify that this variable isn't currently used.
        private const bool _stopping = false;

        internal bool Stopping
        {
            get
            {
                return _stopping;
            }
        }

        List<CommandProcessorBase> commandsToExecute;

        public PipelineProcessor()
        {
            commandsToExecute = new List<CommandProcessorBase>();
        }

        public void Add(CommandProcessorBase commandProcessor)
        {
            commandsToExecute.Add(commandProcessor);
        }

        public Array Execute(ExecutionContext context)
        {
            PSObject psObjectCurrent = context.inputStreamReader.Read();
            Collection<PSObject> dataCollection = new Collection<PSObject>() { psObjectCurrent };

            do
            {
                foreach (CommandProcessorBase commandProcessor in commandsToExecute)
                {
                    PipelineCommandRuntime commandRuntime = new PipelineCommandRuntime(this);

                    foreach (PSObject psObject in dataCollection)
                    {
                        // TODO: protect the execution context
                        commandProcessor.ExecutionContext = context;

                        // TODO: replace the Command default runtime to execute callbacks on the pipeline when the object is written to the pipeline and then execute the next cmdlet

                        // TODO: provide a proper command initialization (for parameters and pipeline objects)
                        commandProcessor.CommandRuntime = commandRuntime;

                        commandProcessor.BindArguments(psObject);

                        // TODO: for each entry in pipe
                        // Execute the cmdlet at least once (even if there were nothing in the pipe
                        commandProcessor.ProcessRecord();
                    }
                    commandProcessor.Complete();

                    // TODO: process Error stream

                    dataCollection = new PSObjectPipelineReader(commandRuntime.outputResults).ReadToEnd();
                }
            } while ((psObjectCurrent = context.inputStreamReader.Read()) != null);

            // Write the final result to the output pipeline
            context.outputStreamWriter.Write(dataCollection, true);

            return dataCollection.Cast<object>().ToArray();
        }
    }
}

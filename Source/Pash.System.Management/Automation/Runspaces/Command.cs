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
using Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public sealed class Command
    {
        public string CommandText { get; private set; }
        public bool IsScript { get; private set; }
        public bool UseLocalScope { get; private set; }

        public PipelineResultTypes MergeUnclaimedPreviousCommandResults { get; set; }
        public CommandParameterCollection Parameters { get; private set; }

        public Command(string command)
            : this(command, false, false)
        {
        }

        public Command(string command, bool isScript)
            : this(command, isScript, false)
        {
        }

        public Command(string command, bool isScript, bool useLocalScope)
        {
            CommandText = command;
            IsScript = isScript;
            UseLocalScope = useLocalScope;
            Parameters = new CommandParameterCollection();
            MergeMyResult = PipelineResultTypes.None;
            MergeToResult = PipelineResultTypes.None;
        }

        public void MergeMyResults(PipelineResultTypes myResult, PipelineResultTypes toResult)
        {
            MergeMyResult = myResult;
            MergeToResult = toResult;
        }

        public override string ToString()
        {
            return CommandText;
        }

        // internals
        //internal Command Clone();
        //internal Command(Command command);
        internal CommandProcessorBase CreateCommandProcessor(ExecutionContext executionContext, CommandManager commandFactory, bool addToHistory)
        {
            CommandProcessorBase cmdProcBase = commandFactory.CreateCommandProcessor(this);
            cmdProcBase.ExecutionContext = executionContext;

            if ((Parameters != null) && (Parameters.Count > 0))
            {
                foreach (CommandParameter parameter in Parameters)
                {
                    if (string.IsNullOrEmpty(parameter.Name))
                    {
                        cmdProcBase.AddParameter(parameter.Value);
                    }
                    else
                    {
                        cmdProcBase.AddParameter(parameter.Name, parameter.Value);
                    }
                }
            }

            return cmdProcBase;
        }

        internal PipelineResultTypes MergeMyResult { get; private set; }
        internal PipelineResultTypes MergeToResult { get; private set; }
    }
}

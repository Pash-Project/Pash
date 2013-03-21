// Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

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
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Diagnostics;

namespace System.Management.Automation.Runspaces
{
    public abstract class Pipeline : IDisposable
    {
        public CommandCollection Commands { get; protected set; }
        public abstract PipelineReader<object> Error { get; }
        public abstract PipelineWriter Input { get; }
        public long InstanceId { get; private set; }
        public abstract bool IsNested { get; }
        public abstract PipelineReader<PSObject> Output { get; }
        public abstract PipelineStateInfo PipelineStateInfo { get; }
        public abstract Runspace Runspace { get; }
        public bool SetPipelineSessionState { get; set; }

        public abstract event EventHandler<PipelineStateEventArgs> StateChanged;

        public abstract Pipeline Copy();
        protected virtual void Dispose(bool disposing) { throw new NotImplementedException(); }

        [DebuggerStepThrough]
        public Collection<PSObject> Invoke()
        {
            return Invoke(new object[] { });
        }

        public abstract Collection<PSObject> Invoke(IEnumerable input);
        public abstract void InvokeAsync();
        public abstract void Stop();
        public abstract void StopAsync();

        // internals
        internal Pipeline()
        {
            Commands = new CommandCollection();
        }

        #region IDisposable Members

        public void Dispose()
        {
            // TODO: implement Pipeline.Dispose
        }

        #endregion
    }
}

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
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Resources;
using System.Collections;
using System.Threading;
using ExecutionContext = Pash.Implementation.ExecutionContext;

namespace System.Management.Automation
{
    public class Cmdlet : InternalCommand
    {
        internal string _ParameterSetName { get; set; }

        protected Cmdlet()
        {
        }

        public bool Stopping
        {
            get
            {
                return IsStopping;
            }
        }

        protected virtual void BeginProcessing()
        {
        }

        protected virtual void EndProcessing()
        {
        }

        public virtual string GetResourceString(string baseName, string resourceId)
        {
            ResourceManager resourceManager = new ResourceManager(GetType());
            string str = null;
            try
            {
                str = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
            }
            catch
            {
            }

            return str;
        }

        public IEnumerable Invoke()
        {
            yield return GetResults();
        }

        public IEnumerable<T> Invoke<T>()
        {
            // TODO: implement Invoke<T>
            throw new NotImplementedException();
        }

        protected virtual void ProcessRecord()
        {
        }

        public bool ShouldContinue(string query, string caption)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldContinue(query, caption);
            }
            return true;
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldContinue(query, caption);
            }
            return true;
        }

        public bool ShouldProcess(string target)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(target);
            }
            return true;
        }

        public bool ShouldProcess(string target, string action)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(target, action);
            }
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption);
            }
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            if (CommandRuntime != null)
            {
                return CommandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
            }
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        protected virtual void StopProcessing()
        {
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            if (CommandRuntime == null)
            {
                if (errorRecord.Exception != null)
                {
                    throw errorRecord.Exception;
                }
                throw new InvalidOperationException(errorRecord.ToString());
            }
            CommandRuntime.ThrowTerminatingError(errorRecord);
        }

        public void WriteCommandDetail(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteCommandDetail");
            }
            CommandRuntime.WriteCommandDetail(text);
        }

        public void WriteDebug(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteDebug");
            }
            CommandRuntime.WriteDebug(text);
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteError");
            }
            CommandRuntime.WriteError(errorRecord);
        }

        public void WriteObject(object sendToPipeline)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteObject");
            }
            CommandRuntime.WriteObject(sendToPipeline);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteObject");
            }
            CommandRuntime.WriteObject(sendToPipeline, enumerateCollection);
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteProgress");
            }
            CommandRuntime.WriteProgress(progressRecord);
        }

        internal void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteProgress");
            }
            CommandRuntime.WriteProgress(sourceId, progressRecord);
        }

        public void WriteVerbose(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteVerbose");
            }
            CommandRuntime.WriteVerbose(text);
        }

        public void WriteWarning(string text)
        {
            if (CommandRuntime == null)
            {
                throw new NotImplementedException("WriteWarning");
            }
            CommandRuntime.WriteWarning(text);
        }

        // internals
        internal override void DoBeginProcessing()
        {
            BeginProcessing();
        }

        internal override void DoEndProcessing()
        {
            EndProcessing();
        }

        internal override void DoProcessRecord()
        {
            ProcessRecord();
        }

        internal override void DoStopProcessing()
        {
            StopProcessing();
        }

        internal ArrayList GetResults()
        {
            if (this is PSCmdlet)
            {
                throw new InvalidOperationException("Can't invoke PSCmdlets directly");
            }
            ArrayList outputArrayList = new ArrayList();
            CommandRuntime = new DefaultCommandRuntime(outputArrayList);
            BeginProcessing();
            ProcessRecord();
            EndProcessing();
            return outputArrayList;
        }
    }
}

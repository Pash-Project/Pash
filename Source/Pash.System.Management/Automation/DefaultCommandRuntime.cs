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

using System.Collections;

namespace System.Management.Automation
{
    public class DefaultCommandRuntime : ICommandRuntime
    {
        private ArrayList outputResults;

        public DefaultCommandRuntime(ArrayList outputResults)
        {
            this.outputResults = outputResults;
        }

        #region ICommandRuntime Members

        public Host.PSHost Host
        {
            get { throw new NotImplementedException(); }
        }

        public bool ShouldContinue(string query, string caption)
        {
            return true;
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            return true;
        }

        public bool ShouldProcess(string target)
        {
            return true;
        }

        public bool ShouldProcess(string target, string action)
        {
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            if (errorRecord.Exception != null)
            {
                throw errorRecord.Exception;
            }
            throw new InvalidOperationException(errorRecord.ToString());
        }

        public void WriteCommandDetail(string text)
        {
        }

        public void WriteDebug(string text)
        {
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            if (errorRecord.Exception != null)
            {
                throw errorRecord.Exception;
            }
            throw new InvalidOperationException(errorRecord.ToString());
        }

        public void WriteObject(object sendToPipeline)
        {
            outputResults.Add(sendToPipeline);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!enumerateCollection)
            {
                outputResults.Add(sendToPipeline);
            }
            else
            {
                IEnumerator enumerator = GetEnumerator(sendToPipeline);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        outputResults.Add(enumerator.Current);
                    }
                }
                else
                {
                    outputResults.Add(sendToPipeline);
                }
            }
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
        }

        public void WriteVerbose(string text)
        {
        }

        public void WriteWarning(string text)
        {
        }

        #endregion

        private IEnumerator GetEnumerator(object obj)
        {
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            IEnumerator enumerator = obj as IEnumerator;
            if (obj != null)
            {
                return enumerator;
            }

            return null;
        }
    }
}

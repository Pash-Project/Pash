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
using System.Diagnostics;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class ProcessBaseCommand : Cmdlet
    {
        internal enum MatchType
        {
            All,
            ByName,
            ById,
            ByInput
        }

        [Parameter(ParameterSetName = "InputObject", Mandatory = true, ValueFromPipeline = true)]
        public Process[] InputObject
        {
            get
            {
                return _input;
            }
            set
            {
                _matchType = MatchType.ByInput;
                _input = value;
            }
        }

        private Process[] _input;
        internal MatchType _matchType;
        internal int[] _processIds;
        internal string[] _processNames;

        internal Process[] AllProcesses
        {
            get
            {
                return Process.GetProcesses();
            }
        }

        internal List<Process> FindProcesses()
        {
            List<Process> foundProcesses = null;
            switch (_matchType)
            {
                case MatchType.ById:
                    foundProcesses = FindProcessesByIds();
                    break;

                case MatchType.ByInput:
                    foundProcesses = FindProcessesByInput();
                    break;

                default:
                    foundProcesses = FindProcessesByNames();
                    break;
            }
            foundProcesses.Sort(new Comparison<Process>(ProcessBaseCommand.ProcessComparer));
            return foundProcesses;
        }

        internal List<Process> FindProcessesByIds()
        {
            List<Process> foundProcesses = new List<Process>();

            if (_processIds == null)
            {
                throw new NullReferenceException("ProcessIDs can't be null");
            }
            foreach (int id in _processIds)
            {
                Process process = null;
                try
                {
                    process = Process.GetProcessById(id);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, id));
                    return foundProcesses;
                }
                foundProcesses.Add(process);
            }

            return foundProcesses;
        }

        internal List<Process> FindProcessesByNames()
        {
            List<Process> foundProcesses = new List<Process>();

            if (_processNames == null)
            {
                return new List<Process>(AllProcesses);
            }
            else
            {
                foreach (string name in _processNames)
                {
                    WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
                    bool bFound = false;

                    foreach (Process process in AllProcesses)
                    {
                        if (pattern.IsMatch(GetProcessName(process)))
                        {
                            bFound = true;
                            foundProcesses.Add(process);
                        }
                    }

                    if (!bFound && !WildcardPattern.ContainsWildcardCharacters(name))
                    {
                        WriteError(new ErrorRecord(new Exception("Can't find process for name: " + name), "", ErrorCategory.ObjectNotFound, name));
                    }
                }
            }
            return foundProcesses;
        }

        internal List<Process> FindProcessesByInput()
        {
            List<Process> foundProcesses = new List<Process>();

            if (InputObject == null)
            {
                throw new NullReferenceException("InputObject can't be null");
            }
            foreach (Process process in InputObject)
            {
                ProcessRefresh(process);
                foundProcesses.Add(process);
            }

            return foundProcesses;
        }

        private static int ProcessComparer(Process p1, Process p2)
        {
            int num = string.Compare(GetProcessName(p1), GetProcessName(p2), StringComparison.CurrentCultureIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            return GetProcessId(p1) - GetProcessId(p2);
        }

        internal static string GetProcessName(Process process)
        {
            try
            {
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return "";
            }
        }

        internal static int GetProcessId(Process process)
        {
            try
            {
                return process.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return Int32.MinValue;
            }
        }

        internal static void ProcessRefresh(Process process)
        {
            try
            {
                process.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}

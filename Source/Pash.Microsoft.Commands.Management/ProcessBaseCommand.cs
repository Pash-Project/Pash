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
                catch(Exception ex)
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
            catch(Exception ex)
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
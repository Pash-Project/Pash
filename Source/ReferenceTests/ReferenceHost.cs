// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace ReferenceTests
{
    public class ExecutionWithErrorsException : Exception
    {
        public ErrorRecord[] Errors { get; set; }

        public ExecutionWithErrorsException(ErrorRecord[] records)
            : base (records.Length == 0 ? "No errors" : "First error: " + records[0].Exception.Message + " (and maybe more errors)")
        {
            Errors = records;
            if (records.Length == 0)
            {
                return;
            }            
        }
    }

    internal static class ReferenceHost
    {
        public static InitialSessionState InitialSessionState { get; set; }
        public static Runspace LastUsedRunspace { get; private set; }
        public static Collection<PSObject> LastRawResults { get; private set; }
        public static Collection<object> LastRawErrorResults { get; private set; }
        public static string LastResults { get; private set; }


        public static string Execute(string cmd, bool throwOnError = true)
        {
            return Execute(new string[] { cmd }, throwOnError);
        }

        public static string Execute(string[] commands, bool throwOnError = true)
        {
            RawExecute(commands, throwOnError);
            return LastResults;
        }

        public static Collection<PSObject> RawExecute(string cmd, bool throwOnError = true)
        {
            return RawExecute(new string[] { cmd }, throwOnError);
        }

        public static Collection<PSObject> RawExecute(string[] commands, bool throwOnError = true)
        {
            LastRawResults = null;
            LastRawErrorResults = null;
            LastUsedRunspace = InitialSessionState == null ?
                RunspaceFactory.CreateRunspace() : RunspaceFactory.CreateRunspace(InitialSessionState);
            LastUsedRunspace.Open();
            return RawExecuteInLastRunspace(commands, throwOnError);
        }

        public static Collection<PSObject> RawExecuteInLastRunspace(string cmd, bool throwOnError = true)
        {
            return RawExecuteInLastRunspace(new string[] { cmd }, throwOnError);
        }

        public static Collection<PSObject> RawExecuteInLastRunspace(string[] commands, bool throwOnError = true)
        {
            foreach (var command in commands)
            {
                using (var pipeline = LastUsedRunspace.CreatePipeline())
                {
                    pipeline.Commands.AddScript(command, false);
                    try
                    {
                        LastRawResults = pipeline.Invoke();
                    }
                    catch (Exception)
                    {
                        // we need to store the results
                        LastRawResults = pipeline.Output.ReadToEnd();;
                        throw;
                    }
                    finally
                    {
                        LastRawErrorResults = pipeline.Error.ReadToEnd();
                        MergeLastRawResultsToString();
                    }
                    if (throwOnError && LastRawErrorResults.Count > 0)
                    {
                        throw new ExecutionWithErrorsException((from err in LastRawErrorResults
                                                                 select ((PSObject) err).BaseObject as ErrorRecord).ToArray());
                    }
                }
            }
            return LastRawResults;
        }

        private static void MergeLastRawResultsToString()
        {
            LastResults = "";
            if (LastRawResults != null)
           {
                StringBuilder resultstr = new StringBuilder();
                foreach (var curPSObject in LastRawResults)
                {
                    if (curPSObject != null)
                    {
                        resultstr.Append(curPSObject.ToString());
                    }
                    resultstr.Append(Environment.NewLine);
                }
                LastResults = resultstr.ToString();
            }
        }

        internal static void ImportModules(string[] modules)
        {
            if (modules == null)
            {
                ReferenceHost.InitialSessionState = null;
            }
            else
            {
                InitialSessionState sessionState = InitialSessionState.CreateDefault();
                sessionState.ImportPSModule(modules);
                ReferenceHost.InitialSessionState = sessionState;
            }
        }

        public static ErrorRecord[] GetLastRawErrorRecords()
        {
            if (LastRawErrorResults == null)
                return new ErrorRecord[0];

            return (from obj in LastRawErrorResults
                    let error = (PSObject)obj
                    select (ErrorRecord)error.BaseObject).ToArray();
        }

        public static object GetVariableValue(string name)
        {
            return LastUsedRunspace.SessionStateProxy.GetVariable(name);
        }
    }
}

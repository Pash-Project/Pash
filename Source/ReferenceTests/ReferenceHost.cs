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
    internal static class ReferenceHost
    {
        public static InitialSessionState InitialSessionState { get; set; }
        public static Runspace LastUsedRunspace { get; private set; }
        public static Collection<PSObject> LastRawResults { get; private set; }
        public static Collection<object> LastRawErrorResults { get; private set; }
        public static string LastResults { get; private set; }


        public static string Execute(string cmd)
        {
            return Execute(new string[] { cmd });
        }

        public static string Execute(string[] commands)
        {
            LastResults = "";
            try
            {
                RawExecute(commands);
            }
            finally
            {
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
            return LastResults;
        }

        public static Collection<PSObject> RawExecute(string cmd, bool throwMethodInvocationException = true)
        {
            return RawExecute(new string[] { cmd }, throwMethodInvocationException);
        }

        public static Collection<PSObject> RawExecute(string[] commands, bool throwMethodInvocationException = true)
        {
            LastRawResults = null;
            LastUsedRunspace = InitialSessionState == null ?
                RunspaceFactory.CreateRunspace() : RunspaceFactory.CreateRunspace(InitialSessionState);
            LastUsedRunspace.Open();
            foreach (var command in commands)
            {
                using (var pipeline = LastUsedRunspace.CreatePipeline())
                {
                    pipeline.Commands.AddScript(command, true);
                    try
                    {
                        LastRawResults = pipeline.Invoke();
                    }
                    catch (Exception)
                    {
                        LastRawResults = pipeline.Output.ReadToEnd();
                        if (pipeline.Error.Count > 0)
                        {
                            LastRawErrorResults = pipeline.Error.ReadToEnd();
                        }
                        throw;
                    }
                    if (throwMethodInvocationException && pipeline.Error.Count > 0)
                    {
                        throw new MethodInvocationException(String.Join(Environment.NewLine, pipeline.Error.ReadToEnd()));
                    }
                }
            }
            return LastRawResults;
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
    }
}

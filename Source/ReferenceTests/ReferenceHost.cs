// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace ReferenceTests
{
    internal static class ReferenceHost
    {
        public static InitialSessionState InitialSessionState { get; set; }
        public static Runspace LastUsedRunspace { get; private set; }


        public static string Execute(string cmd)
        {
            return Execute(new string[] { cmd });
        }

        public static string Execute(string[] commands)
        {
            Collection<PSObject> results = null;
            StringBuilder resultstr = new StringBuilder();

            LastUsedRunspace = InitialSessionState == null ?
                RunspaceFactory.CreateRunspace() : RunspaceFactory.CreateRunspace(InitialSessionState);
            LastUsedRunspace.Open();
            using (var pipeline = LastUsedRunspace.CreatePipeline())
            {
                foreach (var command in commands)
                {
                    pipeline.Commands.AddScript(command, false);
                }
                results = pipeline.Invoke();
                if (pipeline.Error.Count > 0)
                {
                    throw new MethodInvocationException(String.Join(Environment.NewLine, pipeline.Error.ReadToEnd()));
                }
            }
            if (results == null)
            {
                return "";
            }
            foreach (var curPSObject in results)
            {
                if (curPSObject != null)
                {
                    resultstr.Append(curPSObject.ToString());
                }
                resultstr.Append(Environment.NewLine);
            }
            return resultstr.ToString();
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
    }
}

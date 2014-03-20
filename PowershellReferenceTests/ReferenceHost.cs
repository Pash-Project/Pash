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
                    throw new MethodInvocationException(pipeline.Error.ToString());
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
    }
}

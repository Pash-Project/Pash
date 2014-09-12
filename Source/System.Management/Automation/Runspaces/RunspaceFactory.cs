// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation.Host;
using Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public static class RunspaceFactory
    {
        public static RunspaceConfiguration DefaultRunspaceConfiguration
        {
            get { return new LocalRunspaceConfiguration(); }
        }

        public static Runspace CreateRunspace()
        {
            return CreateRunspace(InitialSessionState.CreateDefault());
        }

        public static Runspace CreateRunspace(PSHost host)
        {
            return CreateRunspace(host, InitialSessionState.CreateDefault());
        }

        public static Runspace CreateRunspace(RunspaceConfiguration runspaceConfiguration)
        {
            return CreateRunspace(new LocalHost(), runspaceConfiguration);
        }

        public static Runspace CreateRunspace(PSHost host, RunspaceConfiguration runspaceConfiguration)
        {
            return new LocalRunspace(host, runspaceConfiguration);
        }

        public static Runspace CreateRunspace(PSHost host, InitialSessionState initialSessionState)
        {
            return new LocalRunspace(host, initialSessionState);
        }

        public static Runspace CreateRunspace(InitialSessionState initialSessionState)
        {
            PSHost host = new LocalHost(); // DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture);
            return RunspaceFactory.CreateRunspace(host, initialSessionState);
        }

    }
}

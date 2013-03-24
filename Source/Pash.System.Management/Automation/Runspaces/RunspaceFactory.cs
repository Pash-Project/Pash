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
            return CreateRunspace(DefaultRunspaceConfiguration);
        }

        public static Runspace CreateRunspace(PSHost host)
        {
            return CreateRunspace(host, DefaultRunspaceConfiguration);
        }

        public static Runspace CreateRunspace(RunspaceConfiguration runspaceConfiguration)
        {
            return CreateRunspace(new LocalHost(), runspaceConfiguration);
        }

        public static Runspace CreateRunspace(PSHost host, RunspaceConfiguration runspaceConfiguration)
        {
            return new LocalRunspace(host, runspaceConfiguration);
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

namespace System.Management.Automation.Runspaces
{
    public abstract class RunspaceConfiguration
    {
        private RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> cmdlets;

        public abstract string ShellId
        {
            get;
        }

        protected RunspaceConfiguration()
        {
        }

        public virtual RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> Cmdlets
        {
            get
            {
                if (this.cmdlets == null)
                {
                    this.cmdlets = new RunspaceConfigurationEntryCollection<CmdletConfigurationEntry>();
                }

                return this.cmdlets;
            }
        }

        public virtual RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> Providers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal TypeTable TypeTable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual RunspaceConfigurationEntryCollection<TypeConfigurationEntry> Types
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual RunspaceConfigurationEntryCollection<FormatConfigurationEntry> Formats
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> Scripts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> InitializationScripts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry> Assemblies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual AuthorizationManager AuthorizationManager
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static RunspaceConfiguration Create(string assemblyName)
        {
            return null;
        }

        public static RunspaceConfiguration Create(string consoleFilePath, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return null;
        }

        public static RunspaceConfiguration Create()
        {
            return RunspaceFactory.DefaultRunspaceConfiguration;
        }

        public PSSnapInInfo AddPSSnapIn(string name, out PSSnapInException warning)
        {
            throw new NotImplementedException();
        }

        public PSSnapInInfo RemovePSSnapIn(string name, out PSSnapInException warning)
        {
            throw new NotImplementedException();
        }

    }
}

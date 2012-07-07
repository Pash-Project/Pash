using System;
using System.Management.Automation;

namespace System.Management.Automation.Runspaces
{
    public abstract class RunspaceConfiguration
    {
        protected RunspaceConfiguration() 
        {
            // TODO: associate the RunspaceConfiguration with the config file
            Scripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
            Providers = new RunspaceConfigurationEntryCollection<ProviderConfigurationEntry>();
        }

        //public virtual RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry> Assemblies { get { throw new NotImplementedException(); } }
        //public virtual AuthorizationManager AuthorizationManager { get { throw new NotImplementedException(); } }
        //public virtual RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> Cmdlets { get { throw new NotImplementedException(); } }
        //public virtual RunspaceConfigurationEntryCollection<FormatConfigurationEntry> Formats { get { throw new NotImplementedException(); } }
        //public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> InitializationScripts { get { throw new NotImplementedException(); } }
        public virtual RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> Providers { get; private set; }
        public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> Scripts { get; private set; }
        //public abstract string ShellId { get; }
        //public virtual RunspaceConfigurationEntryCollection<TypeConfigurationEntry> Types { get { throw new NotImplementedException(); } }

        //public PSSnapInInfo AddPSSnapIn(string name, out PSSnapInException warning);
        //public static RunspaceConfiguration Create();
        //public static RunspaceConfiguration Create(string assemblyName);
        //public static RunspaceConfiguration Create(string consoleFilePath, out PSConsoleLoadException warnings);
        //public PSSnapInInfo RemovePSSnapIn(string name, out PSSnapInException warning);

        // internals
        //internal void Bind(System.Management.Automation.ExecutionContext executionContext);
        //internal virtual System.Management.Automation.PSSnapInInfo DoAddPSSnapIn(string name, out System.Management.Automation.Runspaces.PSSnapInException warning);
        //internal virtual System.Management.Automation.PSSnapInInfo DoRemovePSSnapIn(string name, out System.Management.Automation.Runspaces.PSSnapInException warning);
        //internal void Initialize(System.Management.Automation.ExecutionContext executionContext);
        //internal void Unbind(System.Management.Automation.ExecutionContext executionContext);
        //internal void UpdateTypes();
        //internal System.Management.Automation.TypeTable TypeTable { get; }
        //internal Microsoft.PowerShell.Commands.Internal.Format.TypeInfoDataBaseManager FormatDBManager { set; get; }
    }
}

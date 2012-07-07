namespace System.Management.Automation.Runspaces
{
    public sealed class ProviderConfigurationEntry : RunspaceConfigurationEntry
    {
        public ProviderConfigurationEntry(string name, Type implementingType, string helpFileName)
            : base(name)
        {
            ImplementingType = implementingType;
            HelpFileName = helpFileName;
        }

        internal ProviderConfigurationEntry(string name, Type implementingType, string helpFileName, PSSnapInInfo psSnapinInfo)
            : base(name, psSnapinInfo)
        {
            ImplementingType = implementingType;
            HelpFileName = helpFileName;
        }

        public string HelpFileName { get; private set; }
        public Type ImplementingType { get; private set; }
    }
}
using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class CmdletConfigurationEntry : RunspaceConfigurationEntry
    {
        private Type imptype;
        private string helpFileName;

        public Type ImplementingType
        {
            get
            {
                return this.imptype;
            }
        }
        public string HelpFileName
        {
            get
            {
                return this.helpFileName;
            }
        }

        public CmdletConfigurationEntry(string name, Type implementingType, string helpFileName)
            : base(name)
        {
            this.imptype = implementingType;
            this.helpFileName = helpFileName;
        }
    }
}


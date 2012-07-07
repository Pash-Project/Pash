using System;
using System.Management.Automation;

namespace System.Management.Automation.Runspaces
{
    public abstract class RunspaceConfigurationEntry
    {
        protected RunspaceConfigurationEntry(string name)
        {
            Name = name.Trim();
        }

        public bool BuiltIn { get; private set; }
        public string Name { get; private set; }
        public PSSnapInInfo PSSnapIn { get; private set; }

        // internals
        internal RunspaceConfigurationEntry(string name, PSSnapInInfo psSnapin)
        {
            Name = name;
            PSSnapIn = psSnapin;
        }

        //internal UpdateAction Action { get; }
        //internal UpdateAction _action;
        //internal bool _builtIn;
    }
}

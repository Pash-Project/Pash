using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Provider
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CmdletProviderAttribute : Attribute
    {
        public CmdletProviderAttribute(string providerName, ProviderCapabilities providerCapabilities)
        {
            ProviderName = providerName;
            ProviderCapabilities = providerCapabilities;
        }

        public ProviderCapabilities ProviderCapabilities { get; private set; }
        public string ProviderName { get; private set; }
    }
}

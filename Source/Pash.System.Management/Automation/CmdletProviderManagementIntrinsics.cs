using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class CmdletProviderManagementIntrinsics
    {
        private SessionStateGlobal _sessionState;

        internal CmdletProviderManagementIntrinsics(SessionStateGlobal sessionState)
        {
            _sessionState = sessionState;
        }

        public Collection<ProviderInfo> Get(string name) 
        {
            return _sessionState.GetProvidersByName(name);
        }

        public IEnumerable<ProviderInfo> GetAll() 
        {
            return _sessionState.Providers;
        }

        public ProviderInfo GetOne(string name)
        {
            return _sessionState.GetProviderByName(name);
        }

        // internals
        //internal static bool CheckProviderCapabilities(ProviderCapabilities capability, ProviderInfo provider);
        //internal int Count { get; }
    }
}

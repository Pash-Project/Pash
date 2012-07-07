using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class ChildItemCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal ChildItemCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public Collection<PSObject> Get(string path, bool recurse)
        {
            return _cmdlet.State.SessionStateGlobal.GetChildItems(path, recurse);
        }

        public Collection<string> GetNames(string path, ReturnContainers returnContainers, bool recurse)
        {
            return _cmdlet.State.SessionStateGlobal.GetChildNames(path, returnContainers, recurse);
        }

        public bool HasChild(string path)
        {
            throw new NotImplementedException();
        }

        // internals
        //internal ChildItemCmdletProviderIntrinsics(SessionStateInternal sessionState);
        //internal void Get(string path, bool recurse, CmdletProviderContext context);
        //internal object GetChildItemsDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context);
        //internal void GetNames(string path, ReturnContainers returnContainers, bool recurse, CmdletProviderContext context);
        //internal bool HasChild(string path, CmdletProviderContext context);

        internal Collection<PSObject> Get(string path, bool recurse, ProviderRuntime providerRuntime)
        {
            return _cmdlet.State.SessionStateGlobal.GetChildItems(path, recurse, providerRuntime);
        }

        internal Collection<string> GetNames(string path, ReturnContainers returnContainers, bool recurse, ProviderRuntime providerRuntime)
        {
            return _cmdlet.State.SessionStateGlobal.GetChildNames(path, returnContainers, recurse, providerRuntime);
        }
    }
}

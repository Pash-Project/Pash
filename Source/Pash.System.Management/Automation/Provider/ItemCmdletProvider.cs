using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation.Provider
{
    public abstract class ItemCmdletProvider : DriveCmdletProvider
    {
        protected ItemCmdletProvider()
        {
        }

        protected virtual void ClearItem(string path) { throw new NotImplementedException(); }
        protected virtual object ClearItemDynamicParameters(string path) { throw new NotImplementedException(); }
        protected virtual void GetItem(string path) { throw new NotImplementedException(); }
        protected virtual object GetItemDynamicParameters(string path) { throw new NotImplementedException(); }
        protected virtual void InvokeDefaultAction(string path) { throw new NotImplementedException(); }
        protected virtual object InvokeDefaultActionDynamicParameters(string path) { throw new NotImplementedException(); }
        protected abstract bool IsValidPath(string path);
        protected virtual bool ItemExists(string path) { throw new NotImplementedException(); }
        protected virtual object ItemExistsDynamicParameters(string path) { throw new NotImplementedException(); }
        protected virtual void SetItem(string path, object value) { throw new NotImplementedException(); }
        protected virtual object SetItemDynamicParameters(string path, object value) { throw new NotImplementedException(); }

        // internals
        //internal void ClearItem(string path, CmdletProviderContext context);
        //internal object ClearItemDynamicParameters(string path, CmdletProviderContext context);
        //internal void GetItem(string path, CmdletProviderContext context);
        //internal object GetItemDynamicParameters(string path, CmdletProviderContext context);
        //internal void InvokeDefaultAction(string path, CmdletProviderContext context);
        //internal object InvokeDefaultActionDynamicParameters(string path, CmdletProviderContext context);
        //internal bool IsValidPath(string path, CmdletProviderContext context);
        //internal bool ItemExists(string path, CmdletProviderContext context);
        //internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context);
        //internal void SetItem(string path, object value, CmdletProviderContext context);
        //internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context);

        internal bool ItemExists(string path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;

            return ItemExists(path);
        }

        internal void GetItem(string path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            GetItem(path);
        }
    }
}

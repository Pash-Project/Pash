// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        protected virtual void ClearItem(Path path) { throw new NotImplementedException(); }
        protected virtual object ClearItemDynamicParameters(Path path) { throw new NotImplementedException(); }
        protected virtual void GetItem(Path path) { throw new NotImplementedException(); }
        protected virtual object GetItemDynamicParameters(Path path) { throw new NotImplementedException(); }
        protected virtual void InvokeDefaultAction(Path path) { throw new NotImplementedException(); }
        protected virtual object InvokeDefaultActionDynamicParameters(Path path) { throw new NotImplementedException(); }
        protected abstract bool IsValidPath(Path path);
        protected virtual bool ItemExists(Path path) { throw new NotImplementedException(); }
        protected virtual object ItemExistsDynamicParameters(Path path) { throw new NotImplementedException(); }
        protected virtual void SetItem(Path path, object value) { throw new NotImplementedException(); }
        protected virtual object SetItemDynamicParameters(Path path, object value) { throw new NotImplementedException(); }

        // internals
        //internal void ClearItem(Path path, CmdletProviderContext context);
        //internal object ClearItemDynamicParameters(Path path, CmdletProviderContext context);
        //internal void GetItem(Path path, CmdletProviderContext context);
        //internal object GetItemDynamicParameters(Path path, CmdletProviderContext context);
        //internal void InvokeDefaultAction(Path path, CmdletProviderContext context);
        //internal object InvokeDefaultActionDynamicParameters(Path path, CmdletProviderContext context);
        //internal bool IsValidPath(Path path, CmdletProviderContext context);
        //internal bool ItemExists(Path path, CmdletProviderContext context);
        //internal object ItemExistsDynamicParameters(Path path, CmdletProviderContext context);
        //internal void SetItem(Path path, object value, CmdletProviderContext context);
        //internal object SetItemDynamicParameters(Path path, object value, CmdletProviderContext context);

        internal bool ItemExists(Path path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;

            return ItemExists(path);
        }

        internal void GetItem(Path path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            GetItem(path);
        }
    }
}

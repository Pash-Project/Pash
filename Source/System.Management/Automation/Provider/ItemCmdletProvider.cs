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

        protected virtual void ClearItem(string path) { throw new NotImplementedException(); }
        protected virtual object ClearItemDynamicParameters(string path) { throw new NotImplementedException(); }

        protected virtual string[] ExpandPath (string path) { throw new NotImplementedException(); }

        protected virtual void GetItem(string path) { throw new NotImplementedException(); }
        protected virtual object GetItemDynamicParameters(string path) { throw new NotImplementedException(); }

        protected virtual void InvokeDefaultAction(string path) { throw new NotImplementedException(); }
        protected virtual object InvokeDefaultActionDynamicParameters(string path) { throw new NotImplementedException(); }

        protected abstract bool IsValidPath(string path);

        protected virtual bool ItemExists(string path) { throw new NotImplementedException(); }
        protected virtual object ItemExistsDynamicParameters(string path) { throw new NotImplementedException(); }

        protected virtual void SetItem(string path, object value) { throw new NotImplementedException(); }
        protected virtual object SetItemDynamicParameters(string path, object value) { throw new NotImplementedException(); }

        /*
         * While the functions above are stubs to be implemented by the concrete provider, we basically
         * have the same functions to invoke that stubs, but setting the runtime before. The code here is tedious,
         * but I don't think there is a good way to avoid this if we want the functions above to still be "protected".
         * Also we might want to add more behavior ord error checking to be executed before or after the stubs, so
         * let's do it this way.
         */
        internal void ClearItem(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            ClearItem(path);
        }

        internal string[] ExpandPath(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return ExpandPath(path);
        }

        internal object ClearItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return ClearItemDynamicParameters(path);
        }

        internal void GetItem(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            GetItem(path);
        }

        internal object GetItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return GetItemDynamicParameters(path);
        }

        internal void InvokeDefaultAction(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            InvokeDefaultAction(path);
        }

        internal object InvokeDefaultActionDynamicParameters(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return InvokeDefaultActionDynamicParameters(path);
        }

        internal bool IsValidPath(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return IsValidPath(path);
        }

        internal bool ItemExists(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            try
            {
                return ItemExists(path);
            }
            catch (ItemNotFoundException)
            {
                return false;
            }
        }

        internal object ItemExistsDynamicParameters(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return ItemExistsDynamicParameters(path);
        }

        internal void SetItem(string path, object value, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            SetItem(path, value);
        }

        internal object SetItemDynamicParameters(string path, object value, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return SetItemDynamicParameters(path, value);
        }
    }
}

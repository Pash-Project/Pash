// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Management;

namespace System.Management.Automation.Provider
{
    public abstract class ContainerCmdletProvider : ItemCmdletProvider
    {
        protected ContainerCmdletProvider()
        {
        }

        protected virtual void CopyItem(string path, string copyPath, bool recurse) { throw new NotImplementedException(); }
        protected virtual object CopyItemDynamicParameters(string path, string destination, bool recurse) { throw new NotImplementedException(); }
        protected virtual void GetChildItems(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual object GetChildItemsDynamicParameters(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual void GetChildNames(string path, ReturnContainers returnContainers) { throw new NotImplementedException(); }
        protected virtual object GetChildNamesDynamicParameters(string path) { throw new NotImplementedException(); }
        protected virtual bool HasChildItems(string path) { throw new NotImplementedException(); }
        protected virtual void NewItem(string path, string itemTypeName, object newItemValue) { throw new NotImplementedException(); }
        protected virtual object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue) { throw new NotImplementedException(); }
        protected virtual void RemoveItem(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual object RemoveItemDynamicParameters(string path, bool recurse) { throw new NotImplementedException(); }
        protected virtual void RenameItem(string path, string newName) { throw new NotImplementedException(); }
        protected virtual object RenameItemDynamicParameters(string path, string newName) { throw new NotImplementedException(); }

        // internals
        internal void CopyItem(string path, string copyPath, bool recurse, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            CopyItem(path, copyPath, recurse);
        }

        //internal object CopyItemDynamicParameters(Path path, Path destination, bool recurse, CmdletProviderContext context);

        internal void GetChildItems(string path, bool recurse, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            GetChildItems(path, recurse);
        }

        //internal object GetChildItemsDynamicParameters(Path path, bool recurse, CmdletProviderContext context);

        internal void GetChildNames(string path, ReturnContainers returnContainers, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            GetChildNames(path, returnContainers);
        }

        //internal object GetChildNamesDynamicParameters(Path path, CmdletProviderContext context);

        internal bool HasChildItems(string path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return HasChildItems(path);
        }

        internal void NewItem(string path, string type, object newItemValue, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            NewItem(path, type, newItemValue);
        }

        //internal object NewItemDynamicParameters(Path path, Path type, object newItemValue, CmdletProviderContext context);

        internal void RemoveItem(string path, bool recurse, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            RemoveItem(path, recurse);
        }

        //internal object RemoveItemDynamicParameters(Path path, bool recurse, CmdletProviderContext context);

        internal void RenameItem(string path, string newName, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            RenameItem(path, newName);
        }

        //internal object RenameItemDynamicParameters(Path path, string newName, CmdletProviderContext context);
    }
}

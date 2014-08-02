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

        protected virtual void CopyItem(Path path, Path copyPath, bool recurse) { throw new NotImplementedException(); }
        protected virtual object CopyItemDynamicParameters(Path path, Path destination, bool recurse) { throw new NotImplementedException(); }
        protected virtual void GetChildItems(Path path, bool recurse) { throw new NotImplementedException(); }
        protected virtual object GetChildItemsDynamicParameters(Path path, bool recurse) { throw new NotImplementedException(); }
        protected virtual void GetChildNames(Path path, ReturnContainers returnContainers) { throw new NotImplementedException(); }
        protected virtual object GetChildNamesDynamicParameters(Path path) { throw new NotImplementedException(); }
        protected virtual bool HasChildItems(Path path) { throw new NotImplementedException(); }
        protected virtual void NewItem(Path path, string itemTypeName, object newItemValue) { throw new NotImplementedException(); }
        protected virtual object NewItemDynamicParameters(Path path, string itemTypeName, object newItemValue) { throw new NotImplementedException(); }
        protected virtual void RemoveItem(Path path, bool recurse) { throw new NotImplementedException(); }
        protected virtual object RemoveItemDynamicParameters(Path path, bool recurse) { throw new NotImplementedException(); }
        protected virtual void RenameItem(Path path, Path newName) { throw new NotImplementedException(); }
        protected virtual object RenameItemDynamicParameters(Path path, Path newName) { throw new NotImplementedException(); }

        // internals
        internal void CopyItem(Path path, Path copyPath, bool recurse, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            CopyItem(path, copyPath, recurse);
        }

        //internal object CopyItemDynamicParameters(Path path, Path destination, bool recurse, CmdletProviderContext context);

        internal void GetChildItems(Path path, bool recurse, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            GetChildItems(path, recurse);
        }

        //internal object GetChildItemsDynamicParameters(Path path, bool recurse, CmdletProviderContext context);

        internal void GetChildNames(Path path, ReturnContainers returnContainers, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            GetChildNames(path, returnContainers);
        }

        //internal object GetChildNamesDynamicParameters(Path path, CmdletProviderContext context);

        internal bool HasChildItems(Path path, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return HasChildItems(path);
        }

        internal void NewItem(Path path, string type, object newItemValue, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            NewItem(path, type, newItemValue);
        }

        //internal object NewItemDynamicParameters(Path path, Path type, object newItemValue, CmdletProviderContext context);

        internal void RemoveItem(Path path, bool recurse, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            RemoveItem(path, recurse);
        }

        //internal object RemoveItemDynamicParameters(Path path, bool recurse, CmdletProviderContext context);

        internal void RenameItem(Path path, string newName, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            RenameItem(path, newName);
        }

        //internal object RenameItemDynamicParameters(Path path, string newName, CmdletProviderContext context);
    }
}

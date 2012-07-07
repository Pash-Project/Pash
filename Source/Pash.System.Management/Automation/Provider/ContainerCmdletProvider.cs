using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

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
        //internal void CopyItem(string path, string copyPath, bool recurse, CmdletProviderContext context);
        //internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context);
        //internal void GetChildItems(string path, bool recurse, CmdletProviderContext context);
        //internal object GetChildItemsDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal void GetChildNames(string path, ReturnContainers returnContainers, CmdletProviderContext context);
        //internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context);
        //internal bool HasChildItems(string path, CmdletProviderContext context);
        //internal void NewItem(string path, string type, object newItemValue, CmdletProviderContext context);
        //internal object NewItemDynamicParameters(string path, string type, object newItemValue, CmdletProviderContext context);
        //internal void RemoveItem(string path, bool recurse, CmdletProviderContext context);
        //internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal void RenameItem(string path, string newName, CmdletProviderContext context);
        //internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context);

        internal void GetChildItems(string path, bool recurse, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            GetChildItems(path, recurse);
        }
    }
}

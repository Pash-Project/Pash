using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    public sealed class ItemCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public Collection<PSObject> Clear(string path) { throw new NotImplementedException(); }
        public Collection<PSObject> Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers) { throw new NotImplementedException(); }
        public bool Exists(string path) { throw new NotImplementedException(); }
        public Collection<PSObject> Get(string path) { throw new NotImplementedException(); }
        public void Invoke(string path) { throw new NotImplementedException(); }
        public bool IsContainer(string path) { throw new NotImplementedException(); }
        public Collection<PSObject> Move(string path, string destination) { throw new NotImplementedException(); }
        public Collection<PSObject> New(string path, string name, string itemTypeName, object content) { throw new NotImplementedException(); }
        public void Remove(string path, bool recurse) { throw new NotImplementedException(); }
        public Collection<PSObject> Rename(string path, string newName) { throw new NotImplementedException(); }
        public Collection<PSObject> Set(string path, object value) { throw new NotImplementedException(); }

        // internals
        //internal void Clear(string path, CmdletProviderContext context);
        //internal object ClearItemDynamicParameters(string path, CmdletProviderContext context);
        //internal void Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers, CmdletProviderContext context);
        //internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context);
        //internal bool Exists(string path, CmdletProviderContext context);
        //internal void Get(string path, CmdletProviderContext context);
        //internal object GetItemDynamicParameters(string path, CmdletProviderContext context);
        //internal void Invoke(string path, CmdletProviderContext context);
        //internal object InvokeItemDynamicParameters(string path, CmdletProviderContext context);
        //internal bool IsContainer(string path, CmdletProviderContext context);
        //internal ItemCmdletProviderIntrinsics(SessionStateInternal sessionState);
        //internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context);
        //internal void Move(string path, string destination, CmdletProviderContext context);
        //internal object MoveItemDynamicParameters(string path, string destination, CmdletProviderContext context);
        //internal void New(string path, string name, string type, object content, CmdletProviderContext context);
        //internal object NewItemDynamicParameters(string path, string type, object content, CmdletProviderContext context);
        //internal void Remove(string path, bool recurse, CmdletProviderContext context);
        //internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context);
        //internal void Rename(string path, string newName, CmdletProviderContext context);
        //internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context);
        //internal void Set(string path, object value, CmdletProviderContext context);
        //internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context);
    }
}

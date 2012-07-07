using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    public sealed class PropertyCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal PropertyCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void Clear(string path, Collection<string> propertyToClear) { throw new NotImplementedException(); }
        public Collection<PSObject> Copy(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) { throw new NotImplementedException(); }
        public Collection<PSObject> Get(string path, Collection<string> providerSpecificPickList) { throw new NotImplementedException(); }
        public Collection<PSObject> Move(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) { throw new NotImplementedException(); }
        public Collection<PSObject> New(string path, string propertyName, string propertyTypeName, object value) { throw new NotImplementedException(); }
        public void Remove(string path, string propertyName) { throw new NotImplementedException(); }
        public Collection<PSObject> Rename(string path, string sourceProperty, string destinationProperty) { throw new NotImplementedException(); }
        public Collection<PSObject> Set(string path, PSObject propertyValue) { throw new NotImplementedException(); }

        // internals
        //internal void Clear(string path, Collection<string> propertyToClear, CmdletProviderContext context);
        //internal object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear, CmdletProviderContext context);
        //internal void Copy(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context);
        //internal object CopyPropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context);
        //internal void Get(string path, Collection<string> providerSpecificPickList, CmdletProviderContext context);
        //internal object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList, CmdletProviderContext context);
        //internal void Move(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context);
        //internal object MovePropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context);
        //internal void New(string path, string propertyName, string type, object value, CmdletProviderContext context);
        //internal object NewPropertyDynamicParameters(string path, string propertyName, string type, object value, CmdletProviderContext context);
        //internal PropertyCmdletProviderIntrinsics(SessionStateInternal sessionState);
        //internal void Remove(string path, string propertyName, CmdletProviderContext context);
        //internal object RemovePropertyDynamicParameters(string path, string propertyName, CmdletProviderContext context);
        //internal void Rename(string path, string sourceProperty, string destinationProperty, CmdletProviderContext context);
        //internal object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty, CmdletProviderContext context);
        //internal void Set(string path, PSObject propertyValue, CmdletProviderContext context);
        //internal object SetPropertyDynamicParameters(string path, PSObject propertyValue, CmdletProviderContext context);
    }
}

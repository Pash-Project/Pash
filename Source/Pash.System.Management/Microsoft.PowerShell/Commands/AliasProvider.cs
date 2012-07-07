using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.PowerShell.Commands;
using System.Management.Automation.Provider;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("Alias", ProviderCapabilities.ShouldProcess)]
    public sealed class AliasProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Alias";

        public AliasProvider()
        {
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            PSDriveInfo item = new PSDriveInfo("Alias", base.ProviderInfo, string.Empty, string.Empty, null);
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            collection.Add(item);
            return collection;
        }

        protected override object NewItemDynamicParameters(string path, string type, object newItemValue)
        {
            return new AliasProviderDynamicParameters();
        }

        protected override object SetItemDynamicParameters(string path, object value)
        {
            return new AliasProviderDynamicParameters();
        }

        internal override bool CanRenameItem(object item)
        {
            throw new NotImplementedException();
        }

        internal override object GetSessionStateItem(string name)
        {
            throw new NotImplementedException();
        }

        internal override System.Collections.IDictionary GetSessionStateTable()
        {
            throw new NotImplementedException();
        }

        internal override object GetValueOfItem(object item)
        {
            throw new NotImplementedException();
        }

        internal override void RemoveSessionStateItem(string name)
        {
            throw new NotImplementedException();
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            throw new NotImplementedException();
        }
    }
}

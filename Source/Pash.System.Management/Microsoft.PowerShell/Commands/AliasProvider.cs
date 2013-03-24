// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo> { new PSDriveInfo("Alias", base.ProviderInfo) };
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

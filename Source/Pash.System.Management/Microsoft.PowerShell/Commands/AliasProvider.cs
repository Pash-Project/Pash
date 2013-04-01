﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.PowerShell.Commands;
using System.Management.Automation.Provider;
using System.Management.Automation;
using System.Management;

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

        protected override object NewItemDynamicParameters(Path path, string type, object newItemValue)
        {
            return new AliasProviderDynamicParameters();
        }

        protected override object SetItemDynamicParameters(Path path, object value)
        {
            return new AliasProviderDynamicParameters();
        }

        internal override bool CanRenameItem(object item)
        {
            throw new NotImplementedException();
        }

        internal override object GetSessionStateItem(Path name)
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

        internal override void RemoveSessionStateItem(Path name)
        {
            throw new NotImplementedException();
        }

        internal override void SetSessionStateItem(Path name, object value, bool writeItem)
        {
            throw new NotImplementedException();
        }
    }
}

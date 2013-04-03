// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("Environment", ProviderCapabilities.ShouldProcess)]
    public sealed class EnvironmentProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Environment";

        public EnvironmentProvider()
        {
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            PSDriveInfo item = new PSDriveInfo("Env", ProviderInfo, string.Empty, string.Empty, null);
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            collection.Add(item);
            return collection;
        }

        internal override object GetSessionStateItem(Path name)
        {
            string environmentVariable = Environment.GetEnvironmentVariable(name);
            if (environmentVariable != null)
            {
                return new DictionaryEntry(name, environmentVariable);
            }
            return null;
        }

        internal override IDictionary GetSessionStateTable()
        {
            var dictionary = new Dictionary<string, DictionaryEntry>(StringComparer.CurrentCultureIgnoreCase);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                dictionary.Add((string)entry.Key, entry);
            }
            return dictionary;
        }

        internal override void RemoveSessionStateItem(Path name)
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        internal override void SetSessionStateItem(Path path, object value, bool writeItem)
        {
            if (value == null)
            {
                Environment.SetEnvironmentVariable(path, null);
            }
            else
            {
                if (value is DictionaryEntry)
                {
                    value = ((DictionaryEntry)value).Value;
                }
                string str = value as string;
                if (str == null)
                {
                    str = PSObject.AsPSObject(value).ToString();
                }
                Environment.SetEnvironmentVariable(path, str);
                DictionaryEntry item = new DictionaryEntry(path, str);
                if (writeItem)
                {
                    WriteItemObject(item, path, false);
                }
            }
        }
    }
}

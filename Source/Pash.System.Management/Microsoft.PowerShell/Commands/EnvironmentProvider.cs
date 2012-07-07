using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        internal override object GetSessionStateItem(string name)
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

        internal override void RemoveSessionStateItem(string name)
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            if (value == null)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
            else
            {
                if (value is DictionaryEntry)
                {
                    value = ((DictionaryEntry) value).Value;
                }
                string str = value as string;
                if (str == null)
                {
                    str = PSObject.AsPSObject(value).ToString();
                }
                Environment.SetEnvironmentVariable(name, str);
                DictionaryEntry item = new DictionaryEntry(name, str);
                if (writeItem)
                {
                    WriteItemObject(item, name, false);
                }
            }
        }
    }
}
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public class PSSnapInInfo
    {
        public string ApplicationBase { get; private set; }
        public string AssemblyName { get; private set; }
        public string Description { get; private set; }
        public Collection<string> Formats { get; private set; }
        public bool IsDefault { get; private set; }
        public bool LogPipelineExecutionDetails { get; set; }
        public string ModuleName { get; private set; }
        public string Name { get; private set; }
        public Version PSVersion { get; private set; }
        public Collection<string> Types { get; private set; }
        public string Vendor { get; private set; }
        public Version Version { get; private set; }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        // internals
        //internal PSSnapInInfo Clone();
        //internal void LoadIndirectResources();
        internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string vendor)
        {
            Name = name;
            IsDefault = isDefault;
            ApplicationBase = applicationBase;
            AssemblyName = assemblyName;
            ModuleName = moduleName;
            PSVersion = psVersion;
            Version = version;
            Types = types;
            Formats = formats;
            Description = description;
            Vendor = vendor;
        }

        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string descriptionIndirect, string vendor, string vendorFallback, string vendorIndirect, string customPSSnapInType);
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string vendor, string vendorFallback, string customPSSnapInType);
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string descriptionFallback, string vendorFallback, string customPSSnapInType);
        //internal static void VerifyPSSnapInFormatThrowIfError(string mshSnapInId);
        //internal string AbsoluteModulePath { get; }
        //internal string CustomPSSnapInType { get; }
    }
}

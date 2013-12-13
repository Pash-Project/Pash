// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation
{
    public class PSSnapInInfo : IComparable
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
            return Name;
        }

        // internals
        //internal PSSnapInInfo Clone();
        //internal void LoadIndirectResources();

        //TODO: support versions properly
        internal PSSnapInInfo(PSSnapIn snapin, Assembly snapinAssembly, bool isDefault):
            this(snapin.Name, isDefault, snapinAssembly.Location, snapinAssembly.GetName().Name, snapinAssembly.Location,
                 new Version(1, 0), new Version(1, 0), new Collection<string>(snapin.Types),
                 new Collection<string>(snapin.Formats), snapin.Description, snapin.Vendor)
        {
        }

        internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, 
                              string moduleName, Version psVersion, Version version, Collection<string> types, 
                              Collection<string> formats, string description, string vendor)
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

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            //toLower makes sure that two equal objects with same name but othe case-sensitivity return the same value
            return Name.ToLower().GetHashCode();
        }

        #region IComparable members
        public int CompareTo(object obj)
        {
            PSSnapInInfo other = obj as PSSnapInInfo;
            if (other == null)
            {
                throw new PSInvalidOperationException("Can only compare to PSSnapInInfo!");
            }
            return CompareTo(other);
        }

        public int CompareTo(PSSnapInInfo other)
        {
            if (!string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                return Name.CompareTo(other.Name);
            }
            if (string.Equals(AssemblyName, other.AssemblyName, StringComparison.CurrentCultureIgnoreCase))
            {
                return 0;
            }
            return AssemblyName.CompareTo(other.AssemblyName);
        }
        #endregion
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string descriptionIndirect, string vendor, string vendorFallback, string vendorIndirect, string customPSSnapInType);
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string vendor, string vendorFallback, string customPSSnapInType);
        //internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, Version psVersion, Version version, Collection<string> types, Collection<string> formats, string descriptionFallback, string vendorFallback, string customPSSnapInType);
        //internal static void VerifyPSSnapInFormatThrowIfError(string mshSnapInId);
        //internal string AbsoluteModulePath { get; }
        //internal string CustomPSSnapInType { get; }
    }
}

// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;

namespace System.Management.Automation
{
    public sealed class PSModuleInfo : IScopedItem
    {
        internal int NestingDepth { get; set;  }

        public string Path { get; private set; }
        public string Name { get; private set; }
        public SessionState SessionState { get; private set; }
        public ModuleType ModuleType { get; internal set; }
        
        public Dictionary<string, PSVariable> ExportedVariables { get; private set; }
        public Dictionary<string, FunctionInfo> ExportedFunctions { get; private set; }
        public Dictionary<string, AliasInfo> ExportedAliases { get; private set; }
        public Dictionary<string, CmdletInfo> ExportedCmdlets { get; private set; }

        internal bool HasExplicitExports { get; set; }
        internal bool ExportsAreRestrictedByManifest { get; set; }

        internal PSModuleInfo(string path, string name, SessionState sessionState)
        {
            HasExplicitExports = false;
            Path = path;
            Name = name;
            SessionState = sessionState;
            ExportedVariables = new Dictionary<string, PSVariable>();
            ExportedFunctions = new Dictionary<string, FunctionInfo>();
            ExportedAliases = new Dictionary<string, AliasInfo>();
            ExportedCmdlets = new Dictionary<string, CmdletInfo>();
        }

        internal void ValidateExportedMembers()
        {
            // Check if stuff is already exported. If yes, we're fine
            if (HasExplicitExports ||
                ExportedAliases.Count > 0 ||
                ExportedFunctions.Count > 0 ||
                ExportedVariables.Count > 0 ||
                ExportedCmdlets.Count > 0)
            {
                return;
            }
            if (ModuleType.Equals(ModuleType.Script))
            {
                foreach (var fun in SessionState.Function.GetAllLocal())
                {
                    ExportedFunctions.Add(fun.Key, fun.Value);
                }
            }
            else if (ModuleType.Equals(ModuleType.Binary))
            {
                foreach (var cmdlet in SessionState.Cmdlet.GetAllLocal())
                {
                    ExportedCmdlets.Add(cmdlet.Key, cmdlet.Value);
                }
            }
        }

        #region Metadata Properties
        private static readonly List<string> _metadataPropertyNames = new List<string> {
            "AccessMode", "Author", "ClrVersion", "CompanyName", "Copyright", "Description", "DotNetFrameworkVersion",
            "FileList", "Guid", "HelpInfoUri", "ModuleList", "PowerShellHostName", "PowerShellHostVersion",
            "PowerShellVersion", "PrivateData", "ProcessorArchitecture", "ProjectUri", "ReleaseNotes",
            "RepositorySourceLocation", "RequiredAssemblies"
        };
        private static Dictionary<string, PropertyInfo> _metadataProperties;
        private static Dictionary<string, PropertyInfo> MetdataProperties
        {
            get
            {
                if (_metadataProperties == null)
                {
                    _metadataProperties = new Dictionary<string, PropertyInfo>();
                    var type = typeof(PSModuleInfo);
                    foreach (var propName in _metadataPropertyNames)
                    {
                        _metadataProperties[propName] = type.GetProperty(propName);
                    }
                }
                return _metadataProperties;
            }
        }

        internal void SetMetadata(Hashtable metadata)
        {
            // TODO: more validation
            // setting all metadata properties by hand is tedious, we use reflection
            foreach (var propPair in MetdataProperties)
            {
                if (!metadata.ContainsKey(propPair.Key))
                {
                    continue;
                }
                var value = LanguagePrimitives.ConvertTo(metadata[propPair.Key], propPair.Value.PropertyType);
                propPair.Value.SetValue(this, value, null);
            }
            // module version needs to be set extra, because it has a different name in the Hashtable
            // the Version seems to be important when loading a module (e.g. for checking if it needs to
            // be reloaded) and is therefore never overwritten by nested modules (as a nested manifest)
            if (metadata.ContainsKey("ModuleVersion") && Version == null)
            {
                Version = LanguagePrimitives.ConvertTo<Version>(metadata["ModuleVersion"]);
            }
        }

        public ModuleAccessMode AccessMode { get; internal set; }

        public string Author { get; internal set; }

        public Version ClrVersion { get; internal set; }

        public string CompanyName { get; internal set; }

        public string Copyright { get; internal set; }

        public string Description { get; internal set; }

        public Version DotNetFrameworkVersion { get; internal set; }

        public IEnumerable<string> FileList { get; internal set; }

        public Guid Guid { get; internal set; }

        public object HelpInfoUri { get; internal set; }

        public IEnumerable<Object> ModuleList { get; internal set; }

        public string PowerShellHostName { get; internal set; }

        public Version PowerShellHostVersion { get; internal set; }

        public Version PowerShellVersion { get; internal set; }

        public object PrivateData { get; internal set; }

        public ProcessorArchitecture ProcessorArchitecture { get; internal set; }

        public string ProjectUri { get; internal set; }

        public string ReleaseNotes { get; internal set; }

        public string RepositorySourceLocation { get; internal set; }

        public IEnumerable<String> RequiredAssemblies { get; internal set; }

        public Version Version { get; internal set; }

        #endregion

        #region IScopedItem Members

        public string ItemName
        {
            get { return Path; } // spec says: either path to module file or global identifier. so it's unique
        }

        public ScopedItemOptions ItemOptions
        {
            get { return ScopedItemOptions.None; }
            set { throw new NotImplementedException("Setting scope options for PSModuleInfo is not supported"); }
        }
        #endregion

    }
}

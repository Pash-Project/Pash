// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class PSModuleInfo : IScopedItem
    {

        public string Path { get; private set; }
        public string Name { get; private set; }
        public SessionState SessionState { get; private set; }
        
        public Dictionary<string, PSVariable> ExportedVariables { get; private set; }
        public Dictionary<string, FunctionInfo> ExportedFunctions { get; private set; }
        public Dictionary<string, AliasInfo> ExportedAliases { get; private set; }
        public Dictionary<string, CmdletInfo> ExportedCmdlets { get; private set; }

        internal bool HasExplicitExports { get; set; }

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

        internal void ValidateExportedMembers(bool defaultExportCmdlets, bool defaultExportFunctions)
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
            if (defaultExportFunctions)
            {
                foreach (var fun in SessionState.Function.GetAllLocal())
                {
                    ExportedFunctions.Add(fun.Key, fun.Value);
                }
            }
            if (defaultExportCmdlets)
            {
                throw new NotImplementedException("No support for exporting cmdlets, yet");
            }
        }

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

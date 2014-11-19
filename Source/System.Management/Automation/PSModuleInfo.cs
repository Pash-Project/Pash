// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class PSModuleInfo
    {
        public string Name { get; private set; }
        public SessionState SessionState { get; private set; }
        
        public Dictionary<string, PSVariable> ExportedVariables { get; private set; }
        public Dictionary<string, FunctionInfo> ExportedFunctions { get; private set; }
        public Dictionary<string, AliasInfo> ExportedAliases { get; private set; }
        public Dictionary<string, CmdletInfo> ExportedCmdlets { get; private set; }

        public PSModuleInfo(string name, SessionState sessionState)
        {
            Name = name;
            SessionState = sessionState;
            ExportedVariables = new Dictionary<string, PSVariable>();
            ExportedFunctions = new Dictionary<string, FunctionInfo>();
            ExportedAliases = new Dictionary<string, AliasInfo>();
            ExportedCmdlets = new Dictionary<string, CmdletInfo>();
        }

        internal void ExportMembers(bool exportCmdlets, bool exportFunctions)
        {
            // Check if stuff is already exported. If yes, we're fine
            if (ExportedAliases.Count > 0 ||
                ExportedFunctions.Count > 0 ||
                ExportedVariables.Count > 0 ||
                ExportedCmdlets.Count > 0)
            {
                return;
            }
            if (exportFunctions)
            {
                foreach (var fun in SessionState.Function.GetAllLocal())
                {
                    ExportedFunctions.Add(fun.Key, fun.Value);
                }
            }
            if (exportCmdlets)
            {
                throw new NotImplementedException("No support for exporting cmdlets, yet");
            }
        } 
    }
}

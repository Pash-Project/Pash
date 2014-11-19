using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Management.Automation
{
    internal class ModuleIntrinsics
    {
        private SessionState _sessionState;

        public Dictionary<string, PSModuleInfo> ModuleTable { get; private set; }

        public ModuleIntrinsics(SessionState sessionState)
        {
            _sessionState = sessionState;
            ModuleTable = new Dictionary<string, PSModuleInfo>();
        }

        public void Add(PSModuleInfo module)
        {
            ModuleTable[module.Name] = module;
        }

        public void ImportMembers(PSModuleInfo module)
        {
            foreach (var fun in module.ExportedFunctions.Values)
            {
                fun.Module = module;
                _sessionState.Function.Set(fun);
            }
            foreach (var variable in module.ExportedVariables.Values)
            {
                _sessionState.PSVariable.Set(variable);
            }
            foreach (var alias in module.ExportedAliases.Values)
            {
                alias.Module = module;
                _sessionState.Alias.Set(alias, "local");
            }
            // TODO: enable scoped cmdlets
            /*
            foreach (var cmdlet in module.ExportedCmdlets.Values)
            {
                cmdlet.Module = module;
                _sessionState.Cmdlet.Set(cmdlet, "local");
            }
            */
        }
    }
}


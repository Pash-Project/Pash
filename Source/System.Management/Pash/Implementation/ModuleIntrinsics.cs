using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;

namespace Pash.Implementation
{
    internal class ModuleIntrinsics
    {
        /*
         * Not that module intrinsics will use the ModuleInfo.Path as the name (see definition of PSModuleInfo.ItemName)
         * because we can have multiple modules with the same name, but not with the same path (which is also a guid).
         * Make sure to keep that in mind when working with this class.
         */
        private SessionStateScope<PSModuleInfo> _scope;

        public ModuleIntrinsics(SessionStateScope<PSModuleInfo> scope)
        {
            _scope = scope;
        }

        public void Add(PSModuleInfo module, string scope)
        {
            var targetScope = _scope.GetScope(scope, false, _scope);
            targetScope.SessionState.LoadedModules.ImportMembers(module);
            _scope.SetAtScope(module, scope, true);
        }

        public void Remove(PSModuleInfo module)
        {
            RemoveMembers(module);
            // remove the module from all scopes upwards the hierarchy
            foreach (var curScope in _scope.HierarchyIterator)
            {
                if (curScope.HasLocal(module.ItemName))
                {
                    curScope.RemoveLocal(module.ItemName);
                }
            }
        }

        public PSModuleInfo Get(string path)
        {
            return _scope.Get(path, false);
        }

        public Dictionary<string, PSModuleInfo> GetAll()
        {
            return _scope.GetAll();
        }

        private void ImportMembers(PSModuleInfo module)
        {
            foreach (var fun in module.ExportedFunctions.Values)
            {
                fun.Module = module;
                _scope.SessionState.Function.Set(fun);
            }
            foreach (var variable in module.ExportedVariables.Values)
            {
                variable.Module = module;
                _scope.SessionState.PSVariable.Set(variable);
            }
            foreach (var alias in module.ExportedAliases.Values)
            {
                alias.Module = module;
                _scope.SessionState.Alias.Set(alias, "local");
            }
            foreach (var cmdlet in module.ExportedCmdlets.Values)
            {
                cmdlet.Module = module;
                _scope.SessionState.Cmdlet.Set(cmdlet);
            }
        }

        private void RemoveMembers(PSModuleInfo module)
        {
            // check for exported items, check if they are the originals (compare module & module path) and remove them
            foreach (var fun in module.ExportedFunctions.Values)
            {
                var foundFun = _scope.SessionState.Function.Get(fun.ItemName);
                if (foundFun.Module != null && foundFun.Module.Path.Equals(module.Path))
                {
                    _scope.SessionState.Function.Remove(fun.ItemName);
                }
            }
            foreach (var variable in module.ExportedVariables.Values)
            {
                var foundVar = _scope.SessionState.PSVariable.Get(variable.ItemName);
                if (foundVar.Module != null && foundVar.Module.Path.Equals(module.Path))
                {
                    _scope.SessionState.PSVariable.Remove(foundVar.ItemName);
                }
            }
            foreach (var alias in module.ExportedAliases.Values)
            {
                var foundAlias = _scope.SessionState.Alias.Get(alias.ItemName);
                if (foundAlias.Module != null && foundAlias.Module.Path.Equals(module.Path))
                {
                    _scope.SessionState.Alias.Remove(foundAlias.ItemName);
                }
            }
            foreach (var cmdlet in module.ExportedCmdlets.Values)
            {
                var foundCmdlet = _scope.SessionState.Cmdlet.Get(cmdlet.ItemName);
                if (foundCmdlet.Module != null && foundCmdlet.Module.Path.Equals(module.Path))
                {
                    _scope.SessionState.Cmdlet.Remove(foundCmdlet.ItemName);
                }
            }
        }
    }
}


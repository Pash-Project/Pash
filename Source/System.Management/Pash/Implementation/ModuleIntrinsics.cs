using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;

namespace Pash.Implementation
{
    internal class ModuleIntrinsics : SessionStateIntrinsics<PSModuleInfo>
    {
        /*
         * Not that module intrinsics will use the ModuleInfo.Path as the name (see definition of PSModuleInfo.ItemName)
         * because we can have multiple modules with the same name, but not with the same path (which is also a guid).
         * Make sure to keep that in mind when working with this class.
         */

        // modules are a little different: you can import them either to local, global, or *module* scope.
        // the module scope level is unfortunately only supported with modules, not for other scoped items,
        // so we have to operate a little different here
        internal enum ModuleImportScopes
        {
            Local,
            Module,
            Global
        };

        public ModuleIntrinsics(SessionStateScope<PSModuleInfo> scope) : base(scope, false)
        {
        }

        public void Add(PSModuleInfo module, ModuleImportScopes scope)
        {
            // it's either set at global or module level
            var targetScope = GetModuleImportScope(scope);
            targetScope.SetLocal(module, false);
        }

        public void Remove(PSModuleInfo module)
        {
            RemoveMembers(module);
            // remove the module from all scopes upwards the hierarchy
            foreach (var curScope in Scope.HierarchyIterator)
            {
                if (curScope.HasLocal(module.ItemName))
                {
                    curScope.RemoveLocal(module.ItemName);
                }
            }
        }

        internal void ImportMembers(PSModuleInfo module, ModuleImportScopes scope)
        {
            // we either export to
            var targetScope = GetModuleImportScope(scope);
            foreach (var fun in module.ExportedFunctions.Values)
            {
                fun.Module = fun.Module ?? module;
                targetScope.SessionState.Function.Set(fun);
            }
            foreach (var variable in module.ExportedVariables.Values)
            {
                variable.Module = variable.Module ?? module;
                targetScope.SessionState.PSVariable.Set(variable);
            }
            foreach (var alias in module.ExportedAliases.Values)
            {
                alias.Module = alias.Module ?? module;
                targetScope.SessionState.Alias.Set(alias, "local");
            }
            foreach (var cmdlet in module.ExportedCmdlets.Values)
            {
                cmdlet.Module = cmdlet.Module ?? module;
                targetScope.SessionState.Cmdlet.Set(cmdlet);
            }
        }

        private void RemoveMembers(PSModuleInfo module)
        {
            // check for exported items, check if they are the originals (compare module & module path) and remove them
            foreach (var fun in module.ExportedFunctions.Values)
            {
                var foundFun = Scope.SessionState.Function.Get(fun.ItemName);
                if (foundFun.Module != null && foundFun.Module.Path.Equals(module.Path))
                {
                    Scope.SessionState.Function.Remove(fun.ItemName);
                }
            }
            foreach (var variable in module.ExportedVariables.Values)
            {
                var foundVar = Scope.SessionState.PSVariable.Get(variable.ItemName);
                if (foundVar.Module != null && foundVar.Module.Path.Equals(module.Path))
                {
                    Scope.SessionState.PSVariable.Remove(foundVar.ItemName);
                }
            }
            foreach (var alias in module.ExportedAliases.Values)
            {
                var foundAlias = Scope.SessionState.Alias.Get(alias.ItemName);
                if (foundAlias.Module != null && foundAlias.Module.Path.Equals(module.Path))
                {
                    Scope.SessionState.Alias.Remove(foundAlias.ItemName);
                }
            }
            foreach (var cmdlet in module.ExportedCmdlets.Values)
            {
                var foundCmdlet = Scope.SessionState.Cmdlet.Get(cmdlet.ItemName);
                if (foundCmdlet.Module != null && foundCmdlet.Module.Path.Equals(module.Path))
                {
                    Scope.SessionState.Cmdlet.Remove(foundCmdlet.ItemName);
                }
            }
        }

        private SessionStateScope<PSModuleInfo> GetModuleImportScope(ModuleImportScopes scope)
        {
            if (scope.Equals(ModuleImportScopes.Local))
            {
                return Scope;
            }
            var moduleScope = scope.Equals(ModuleImportScopes.Module);
            foreach (var curScope in Scope.HierarchyIterator)
            {
                // check for global scope. Even if we want to get the module scope, this means it doesn't exist
                // or we would have returned it
                if (curScope.ParentScope == null)
                {
                    return curScope;
                }
                // check for module scope
                if (moduleScope && curScope.SessionState.Module != null)
                {
                    return curScope;
                }
            }
            // should never happen as we *must* reach the global scope
            return null;
        }
    }
}


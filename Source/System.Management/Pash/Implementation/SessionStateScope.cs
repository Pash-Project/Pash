using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Management.Automation;
using System.Linq;

namespace Pash.Implementation
{
    internal class SessionStateScope
    {
        public enum ScopeSpecifiers {
            Global,
            Local,
            Script,
            Private
        };

        internal Dictionary<string, AliasInfo> LocalAliases { get; private set; }
        internal Dictionary<string, CommandInfo> LocalFunctions { get; private set; }
        internal Dictionary<string, PSVariable> LocalVariables { get; private set; }
        internal Dictionary<string, PSDriveInfo> LocalDrives { get; private set; }

        public SessionStateScope ParentScope { get; private set; }
        public SessionStateGlobal SessionStateGlobal { get; private set; }
        public bool IsScriptScope { get; set; }

        public SessionStateScope(SessionStateGlobal sessionStateGlobal) : this(null, sessionStateGlobal) {}

        public SessionStateScope(SessionStateScope parentScope) : this(parentScope, parentScope.SessionStateGlobal) {}


        private SessionStateScope(SessionStateScope parentScope, SessionStateGlobal sessionStateGlobal)
        {
            LocalAliases = new Dictionary<string, AliasInfo>(StringComparer.CurrentCultureIgnoreCase);
            LocalFunctions = new Dictionary<string, CommandInfo>(StringComparer.CurrentCultureIgnoreCase);
            LocalVariables = new Dictionary<string, PSVariable>(StringComparer.CurrentCultureIgnoreCase);
            LocalDrives = new Dictionary<string, PSDriveInfo>(StringComparer.CurrentCultureIgnoreCase);
            SessionStateGlobal = sessionStateGlobal;
            IsScriptScope = false;
            ParentScope = parentScope;
            //TODO: care about AllScope items!
            //Just setting the parent scope is a little too easy. We have to copy all AllScope members to this scope!
        }

        internal SessionStateScope GetScope(string specifier, bool numberAllowed = true)
        {
            int scopeLevel = -1;
            SessionStateScope candidate = this;
            //check if the specifier is a number before trying to interprete it as enum
            if (int.TryParse(specifier, out scopeLevel))
            {
                //sometimes a number specifies the relative scope location, but in some contexts this isn't allowed
                //this has to happen *after* trying to parse them as e.g. "0" would be a parsable enum value!
                if (!numberAllowed || scopeLevel < 0)
                {
                    throw new ArgumentException("Invalid scope specifier");
                }
                while (scopeLevel > 0)
                {
                    //make sure we can proceed looking for the next scope
                    if (candidate.ParentScope == null)
                    {
                        throw new ArgumentOutOfRangeException("Exceeded the maximum number of available scopes");
                    }
                    candidate = candidate.ParentScope;
                    scopeLevel--;
                }
                return candidate;
            }
            //it's not a (positive) number, let's check if it's a named scope
            ScopeSpecifiers scopeSpecifier;
            if (!ScopeSpecifiers.TryParse(specifier, true, out scopeSpecifier))
            {
                throw new ArgumentException("Invalid scope specifier");
            }
            return GetScope(scopeSpecifier);
        }

        internal SessionStateScope GetScope(ScopeSpecifiers specifier)
        {
            //if the local scope is meant, return this instance itself
            if (specifier == ScopeSpecifiers.Local || specifier == ScopeSpecifiers.Private)
            {
                return this;
            }
            var candidate = this;
            //otherwise it's a global or script scope specifier
            if (specifier == ScopeSpecifiers.Global || specifier == ScopeSpecifiers.Script)
            {
                while (candidate.ParentScope != null)
                {
                    if (candidate.IsScriptScope && specifier == ScopeSpecifiers.Script)
                    {
                        return candidate;
                    }
                    candidate = candidate.ParentScope;
                }
                //found the scope without a parent: the global scope
                return candidate;
            }
            throw new ArgumentException(String.Format("Invalid scope specifier \"{0}\"", specifier));
        }

        #region drive access
        internal PSDriveInfo GetLocalDrive(string driveName)
        {
            if (LocalDrives.ContainsKey(driveName))
            {
                return LocalDrives[driveName];
            }
            return null;
        }

        internal bool AddLocalDrive(PSDriveInfo drive)
        {
            if (LocalDrives.ContainsKey(drive.Name))
            {
                return false;
            }
            LocalDrives.Add(drive.Name, drive);
            return true;
        }

        internal bool RemoveLocalDrive(string driveName, bool force)
        {
            /* TODO: force is used to remove the drive "although it's in use by the provider"
             * So, we need to find out when a drive is in use and should throw an exception on removal without
             * the "force" parameter being true
             */
            return LocalDrives.Remove(driveName);
        }
        #endregion

        #region variable access
        internal PSVariable GetLocalVariable(string unqualifiedName)
        {
            if (LocalVariables.ContainsKey(unqualifiedName))
            {
                return (PSVariable) LocalVariables[unqualifiedName];
            }
            return null;
        }

        internal void RemoveLocalVariable(string name)
        {
            LocalVariables.Remove(name);
        }

        internal void SetLocalVariable(PSVariable variable)
        {
            RemoveLocalVariable(variable.Name);
            LocalVariables.Add(variable.Name, variable);
        }
        #endregion
    }
}


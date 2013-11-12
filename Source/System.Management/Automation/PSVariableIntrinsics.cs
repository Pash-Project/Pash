// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public sealed class PSVariableIntrinsics
    {
        private SessionStateScope sessionScope;

        internal PSVariableIntrinsics(SessionStateScope scope)
        {
            sessionScope = scope;
        }

        public PSVariable Get(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("The variable name is null.");
            }
            var path = new VariablePath(name);
            var unqualifiedName = path.IsUnqualified ? name : UnqualifyVariableName(name);
            var qualifiedScope = ResolveQualifiedScope(path);
            if (qualifiedScope != null)
            {
                var variable = qualifiedScope.GetLocalVariable(unqualifiedName);
                //return null if it's private
                if (qualifiedScope != sessionScope && variable.IsPrivate)
                {
                    return null;
                }
                return variable;
            }
            var hostingScope = FindHostingScope(unqualifiedName);
            if (hostingScope != null)
            {
                return hostingScope.GetLocalVariable(unqualifiedName);
            }
            return null;
        }

        public object GetValue(string name)
        {
            var variable = Get(name);
            return (variable != null) ? variable.Value : null;
        }

        public object GetValue(string name, object defaultValue)
        {
            return GetValue(name) ?? defaultValue;
        }

        public void Remove(PSVariable variable)
        {
            //no scope specified when passing an object: we only care about the local scope
            if (variable == null)
            {
                throw new ArgumentNullException("The variable is null.");
            }
            //add the local scope specifier to make sure we don't screw up with other variables
            Remove("local:" + variable.Name);
        }

        public void Remove (string name)
        {
            if (name == null) {
                throw new ArgumentNullException("The variable name is null.");
            }
            //difference to get: we don't lookup the variable in parent scopes to remove it.
            var path = new VariablePath(name);
            var unqualifiedName = path.IsUnqualified ? name : UnqualifyVariableName(name);
            var affectedScope = ResolveQualifiedScope(path) ?? FindHostingScope(unqualifiedName);
            if (affectedScope == null)
            {
                return; //not found
            }
            var variable = affectedScope.GetLocalVariable(unqualifiedName);
    
            if (variable == null) //not in the affected scope
            {
                return;
            }
            if (variable.Options.HasFlag(ScopedItemOptions.Constant))
            {
                throw new SessionStateUnauthorizedAccessException("The variable is a constant and cannot be removed.");
            }
            affectedScope.RemoveLocalVariable(unqualifiedName);
        }

        public void Set(PSVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("The variable is null.");
            }
            var original = sessionScope.GetLocalVariable(variable.Name);
            if (original == null)
            {
                sessionScope.SetLocalVariable(variable);
                return;
            }
            if (original.Options.HasFlag(ScopedItemOptions.ReadOnly | ScopedItemOptions.Constant))
            {
                throw new SessionStateUnauthorizedAccessException("The variable is read-only or a constant.");
            }
            original.Value = variable.Value;
            original.Description = variable.Description;
            original.Options = variable.Options;
            sessionScope.SetLocalVariable(original);
        }

        public void Set(string name, object value)
        {
            if (name == null) {
                throw new ArgumentNullException("The variable name is null.");
            }
            var path = new VariablePath(name);
            var unqualifiedName = path.IsUnqualified ? name : UnqualifyVariableName (name);
            var affectedScope = ResolveQualifiedScope(path) ?? sessionScope;
            var original = affectedScope.GetLocalVariable(unqualifiedName);
            var variable = original ?? new PSVariable(unqualifiedName);
            if (variable.Options.HasFlag(ScopedItemOptions.ReadOnly | ScopedItemOptions.Constant))
            {
                throw new SessionStateUnauthorizedAccessException("The variable is read-only or a constant.");
            }
            if (path.IsPrivate && original == null) //only set private flag if this variable is new
            {
                variable.Options |= ScopedItemOptions.Private;
            }
            variable.Value = value;
            affectedScope.SetLocalVariable(variable);
        }

        internal Dictionary<string, PSVariable> GetAll()
        {
            //get a copy of the vars in the local scope first. Note: it also copies the correct comperator
            var visibleVars = new Dictionary<string, PSVariable> (sessionScope.LocalVariables);
            //now check recursively the parent scopes for non-private, not overriden variables
            for (var scope = sessionScope.ParentScope; scope != null; scope = scope.ParentScope)
            {
                foreach (var pair in scope.LocalVariables)
                {
                    if (!visibleVars.ContainsKey(pair.Key) && !pair.Value.IsPrivate)
                    {
                        visibleVars.Add(pair.Key, pair.Value);
                    }
                }
            }
            return visibleVars;
        }

        private SessionStateScope ResolveQualifiedScope(VariablePath path)
        {
            if (!path.IsVariable)
            {
                //TODO: what if this "variable" isn't actually a variable?
                // -> then we have a drive set, instead of a scope. this means... what?
                throw new NotImplementedException();
            }
            if (path.IsScript)
            {
                return sessionScope.GetScope(SessionStateScope.ScopeSpecifiers.Script);
            }
            else if (path.IsGlobal)
            {
                return sessionScope.GetScope(SessionStateScope.ScopeSpecifiers.Global);
            }
            else if (path.IsLocal)
            {
                return sessionScope;
            }
            return null; //no scope explicitly given!
        }

        private string UnqualifyVariableName(string name)
        {
            //return name after ":"
            var parts = name.Split(':');
            if (parts.Length < 2)
            {
                return name;
            }
            return parts[1];
        }

        private SessionStateScope FindHostingScope(string unqualifiedName)
        {
            //iterate through scopes and parents until we find the variable
            for (var candidate = sessionScope; candidate != null; candidate = candidate.ParentScope)
            {
                var variable = candidate.GetLocalVariable(unqualifiedName);
                if (variable == null)
                {
                    continue;
                }
                //make also sure the variable isn't private, if it's from a parent scope!
                if((candidate == sessionScope) || !variable.IsPrivate)
                {
                    return candidate;
                }
            }
            return null; //nothing found
        }
    }
}

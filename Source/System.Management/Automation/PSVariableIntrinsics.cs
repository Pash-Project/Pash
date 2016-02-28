// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public sealed class PSVariableIntrinsics : ISessionStateIntrinsics<PSVariable>
    {
        /* This class is part of the public Powershell API. Therefore we cannot
         * derive from SessionStateIntrinsics<PSVariable>, but use it internally
         * and map some functions to it to conform to the public API
         */
        private SessionStateIntrinsics<PSVariable> _intrinsics;

        internal PSVariableIntrinsics(SessionStateScope<PSVariable> scope)
        {
            _intrinsics = new SessionStateIntrinsics<PSVariable>(scope, true);
        }

        public bool SupportsScopedName { get { return true; } }

        public PSVariable Get(string name)
        {
            var variable = _intrinsics.Get(name);
            if (variable != null)
            {
                ThrowIfVariableIsPrivate(variable);
            }
            return variable;
        }

        public PSVariable GetAtScope(string name, string scope)
        {
            var variable = _intrinsics.GetAtScope(name, scope);
            if (variable != null)
            {
                ThrowIfVariableIsPrivate(variable);
            }
            return variable;
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

        public void Remove(string name)
        {
            _intrinsics.Scope.Remove(name, true);
        }

        public void Set(PSVariable variable, bool force = false)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("The variable is null.");
            }
            var original = _intrinsics.Scope.GetLocal(variable.Name);
            if (original == null)
            {
                _intrinsics.Scope.SetLocal(variable, true);
                return;
            }
            CheckVariableCanBeChanged(original, force);
            original.Value = variable.Value;
            original.Description = variable.Description;
            original.Options = variable.Options;
            _intrinsics.Scope.SetLocal(original, true, force);
        }

        internal void SetAtScope(PSVariable variable, string scope, bool force = false)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("The variable is null.");
            }

            var original = _intrinsics.Scope.GetLocal(variable.Name);
            if (original == null)
            {
                _intrinsics.Scope.SetAtScope(variable, scope, true);
                return;
            }
            //CheckVariableCanBeChanged(original, force);
            //original.Value = variable.Value;
            //original.Description = variable.Description;
            //original.Options = variable.Options;
            //_intrinsics.Scope.SetLocal(original, true, force);
        }

        public void Set(string name, object value)
        {
            var qualName = new SessionStateScope<PSVariable>.QualifiedName(name);
            // check for existing one. if it's not a scope qualified name, make sure to check only local scope
            // because we won't override a parent scope variable with the same name
            var variable = qualName.ScopeSpecifier.Length == 0 ? _intrinsics.Scope.GetLocal(name)
                                                               : _intrinsics.Scope.Get(name, true);
            if (variable == null) // doesn't exist, yet. create
            {
                _intrinsics.Scope.Set(name, new PSVariable(qualName.UnqualifiedName, value), true, true);
                return;
            }
            // make sure it's not read only
            CheckVariableCanBeChanged(variable);
            // only modify the value of the old one
            variable.Value = value;
        }

        public Dictionary<string, PSVariable> Find(string pattern)
        {
            return _intrinsics.Find(pattern);
        }

        public Dictionary<string, PSVariable> GetAll()
        {
            return _intrinsics.GetAll();
        }

        public Dictionary<string, PSVariable> GetAllLocal()
        {
            return _intrinsics.GetAllLocal();
        }

        internal Dictionary<string, PSVariable> GetAllAtScope(string scope)
        {
            return _intrinsics.GetAllAtScope(scope);
        }

        internal Dictionary<string, PSVariable> FindAtScope(string pattern, string scope)
        {
            return _intrinsics.FindAtScope(pattern, scope);
        }

        private static void CheckVariableCanBeChanged(PSVariable variable, bool force = false)
        {
            if ((variable.ItemOptions.HasFlag(ScopedItemOptions.ReadOnly) && !force) ||
                variable.ItemOptions.HasFlag(ScopedItemOptions.Constant))
            {
                var ex = SessionStateUnauthorizedAccessException.CreateVariableNotWritableError(variable);
                throw ex;
            }

            ThrowIfVariableIsPrivate(variable);
        }

        private static void ThrowIfVariableIsPrivate(PSVariable variable)
        {
            if (variable.Visibility != SessionStateEntryVisibility.Private)
            {
                return;
            }

            var exception = new SessionStateException(
                String.Format("Cannot access the variable '${0}' because it is a private variable", variable.Name),
                variable.ItemName,
                SessionStateCategory.Variable);

            string errorId = "VariableIsPrivate";
            var parentException = new ParentContainsErrorRecordException(exception);
            var error = new ErrorRecord(parentException, errorId, ErrorCategory.PermissionDenied, variable.Name);
            exception.ErrorRecord = error;

            throw exception;
        }
    }
}

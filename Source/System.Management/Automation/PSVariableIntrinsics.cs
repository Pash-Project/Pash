// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public sealed class PSVariableIntrinsics
    {
        private SessionStateScope<PSVariable> _scope;

        internal PSVariableIntrinsics(SessionStateScope<PSVariable> variableScope)
        {
            _scope = variableScope;
        }

        public PSVariable Get(string name)
        {
            return _scope.Get(name, true);
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
            _scope.Remove(name, true);
        }

        public void Set(PSVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("The variable is null.");
            }
            var original = _scope.GetLocal(variable.Name);
            if (original == null)
            {
                _scope.SetLocal(variable, true);
                return;
            }
            original.Value = variable.Value;
            original.Description = variable.Description;
            original.Options = variable.Options;
            _scope.SetLocal(original, true);
        }

        public void Set(string name, object value)
        {
            var qualName = new SessionStateScope<PSVariable>.QualifiedName(name);
            _scope.Set(name, new PSVariable(qualName.UnqualifiedName, value), true, true);
        }

        internal IEnumerable<string> Find(string pattern)
        {
            return _scope.Find(pattern, true);
        }

        internal Dictionary<string, PSVariable> GetAll()
        {
            return _scope.GetAll();
        }

    }
}

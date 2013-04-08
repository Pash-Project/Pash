// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public class SessionStateProxy
    {
        private LocalRunspace _runspace;
        internal SessionStateProxy(LocalRunspace runspace)
        {
            _runspace = runspace;
        }

        public object GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            return _runspace.GetVariable(name);
        }

        public void SetVariable(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            _runspace.SetVariable(name, value);
        }
    }
}

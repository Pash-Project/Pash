using System;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class PSVariableIntrinsics
    {
        private SessionStateGlobal _sessionState;

        internal PSVariableIntrinsics(SessionStateGlobal sessionState)
        {
            _sessionState = sessionState;
        }

        public PSVariable Get(string name) 
        {
            return _sessionState.GetVariable(name);
        }

        public object GetValue(string name) 
        {
            return _sessionState.GetVariableValue(name);
        }

        public object GetValue(string name, object defaultValue) 
        {
            return _sessionState.GetVariableValue(name, defaultValue);
        }

        public void Remove(PSVariable variable)
        {
            _sessionState.RemoveVariable(variable);
        }

        public void Remove(string name)
        {
            _sessionState.RemoveVariable(name);
        }

        public void Set(PSVariable variable)
        {
            _sessionState.SetVariable(variable);
        }

        public void Set(string name, object value)
        {
            _sessionState.SetVariable(name, value);
        }

        // internals
        //internal PSVariable GetAtScope(string name, string scope);
        //internal object GetValueAtScope(string name, string scope);
        //internal void RemoveAtScope(PSVariable variable, string scope);
        //internal void RemoveAtScope(string name, string scope);
        //internal void SetAtScope(PSVariable variable, string scope);
        //internal void SetAtScope(string name, object value, string scope);
    }
}

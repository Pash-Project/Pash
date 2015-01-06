using System;
using System.Collections.Generic;

namespace Pash.Implementation
{
    internal class SessionStateIntrinsics<T> : ISessionStateIntrinsics<T> where T : IScopedItem
    {
        internal SessionStateScope<T> Scope { get; private set; }

        public bool SupportsScopedName { get; private set; }

        public SessionStateIntrinsics(SessionStateScope<T> scope, bool supportsScopedName)
        {
            Scope = scope;
            SupportsScopedName = supportsScopedName;
        }

        public virtual T Get(string name)
        {
            return Scope.Get(name, SupportsScopedName);
        }

        public virtual Dictionary<string, T> GetAll()
        {
            return Scope.GetAll();
        }

        public virtual T GetAtScope(string name, string scope)
        {
            return Scope.GetAtScope(name, scope);
        }

        public virtual Dictionary<string, T> GetAllLocal()
        {
            return new Dictionary<string, T>(Scope.Items);
        }

        public virtual Dictionary<string, T> Find(string name)
        {
            return Scope.Find(name, SupportsScopedName);
        }
    }
}


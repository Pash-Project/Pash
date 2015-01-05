using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace Pash.Implementation
{
    internal sealed class AliasIntrinsics : SessionStateIntrinsics<AliasInfo>
    {
        internal AliasIntrinsics(SessionStateScope<AliasInfo> scope) : base(scope, false)
        {
        }

        public bool Exists(string aliasName)
        {
            return (Get(aliasName) != null);
        }

        public void Set(AliasInfo info, string scope)
        {
            Scope.SetAtScope(info, scope, true);
        }

        public void New(AliasInfo info, string scope)
        {
            Scope.SetAtScope(info, scope, false);
        }

        public void Remove(string aliasName, string scope)
        {
            Scope.RemoveAtScope(aliasName, scope);
        }

        internal void Remove(string aliasName)
        {
            Scope.Remove(aliasName, false);
        }
    }
}


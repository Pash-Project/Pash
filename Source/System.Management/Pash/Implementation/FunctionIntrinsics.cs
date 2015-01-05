using System;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Pash.Implementation
{
    internal sealed class FunctionIntrinsics : SessionStateIntrinsics<FunctionInfo>
    {
        internal FunctionIntrinsics(SessionStateScope<FunctionInfo> scope) : base(scope, true)
        {
        }

        public void Set(string name, ScriptBlock function, string description = "")
        {
            Set(name, function, null, description);
        }

        public void Set(string name, ScriptBlock function, IEnumerable<ParameterAst> parameters,
                        string description)
        {
            var qualName = new SessionStateScope<FunctionInfo>.QualifiedName(name);
            var info = new FunctionInfo(qualName.UnqualifiedName, function, parameters);
            info.Description = description;
            Scope.Set(name, info, true, true);
        }

        public void Set(FunctionInfo info)
        {
            Scope.SetAtScope(info, "local", true);
        }

        public void Remove(string name)
        {
            Scope.Remove(name, true);
        }

    }
}


using System;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class AliasAttribute : ParsingBaseAttribute
    {
        public AliasAttribute(params string[] aliasNames)
        {
            AliasNames = new List<string>(aliasNames);
        }

        public IList<string> AliasNames { get; private set; }
    }
}

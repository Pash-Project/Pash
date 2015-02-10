using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Pash.Implementation
{
    public class IncludeExcludeFilter
    {
        private readonly WildcardPattern[] _include;
        private readonly WildcardPattern[] _exclude;

        public bool CanBeIgnored { get; private set; }

        public IncludeExcludeFilter(IList<string> include, IList<string> exclude, bool ignoreFilters)
        {
            CanBeIgnored = ignoreFilters ||
                           ((include == null || include.Count == 0) && (exclude == null || exclude.Count == 0));
            if (CanBeIgnored)
            {
                return;
            }
            // no include set: include everything
            include = include == null || include.Count == 0 ? new [] { "*" } : include;
            _include = WildcardPattern.CreateWildcards(include);
            _exclude = WildcardPattern.CreateWildcards(exclude);
        }

        public bool Accepts(string value)
        {
            if (CanBeIgnored)
            {
                return true;
            }
            return WildcardPattern.IsAnyMatch(_include, value) && !WildcardPattern.IsAnyMatch(_exclude, value);
        }
    }
}


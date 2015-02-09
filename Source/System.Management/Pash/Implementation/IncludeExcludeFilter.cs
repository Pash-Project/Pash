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
            CanBeIgnored = ignoreFilters;
            if (!CanBeIgnored)
            {
                _include = WildcardPattern.CreateWildcards(include);
                _exclude = WildcardPattern.CreateWildcards(exclude);
            }
            CanBeIgnored = _include.Length == 0 && _exclude.Length == 0; // shortcut if no filters set
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


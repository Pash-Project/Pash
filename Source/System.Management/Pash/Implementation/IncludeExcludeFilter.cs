using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Pash.Implementation
{
    public class IncludeExcludeFilter
    {
        bool _ignoreFilters;
        WildcardPattern[] _include;
        WildcardPattern[] _exclude;

        public IncludeExcludeFilter(IList<string> include, IList<string> exclude, bool ignoreFilters)
        {
            _ignoreFilters = ignoreFilters;
            if (!_ignoreFilters)
            {
                _include = WildcardPattern.CreateWildcards(include);
                _exclude = WildcardPattern.CreateWildcards(exclude);
            }
            _ignoreFilters = _include.Length == 0 && _exclude.Length == 0; // shortcut if no filters set
        }

        public bool Accepts(string value)
        {
            if (_ignoreFilters)
            {
                return true;
            }
            return WildcardPattern.IsAnyMatch(_include, value) && !WildcardPattern.IsAnyMatch(_exclude, value);
        }
    }
}


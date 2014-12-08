using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Management.Automation;
using System.Linq;

namespace Pash.Implementation
{
    internal class SessionStateScope<T> where T : IScopedItem
    {
        public enum ScopeSpecifiers
        {
            Global,
            Local,
            Script,
            Private
        }

        //allows foreach statement with scope hierarchies
        public class ScopeHierarchyIterator : IEnumerable<SessionStateScope<T>>
        {
            private SessionStateScope<T> _start;

            public ScopeHierarchyIterator(SessionStateScope<T> start)
            {
                _start = start;
            }

            public IEnumerator<SessionStateScope<T>> GetEnumerator()
            {
                for (var itemSet = _start; itemSet != null; itemSet = itemSet.ParentScope)
                {
                    yield return itemSet;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                for (var itemSet = _start; itemSet != null; itemSet = itemSet.ParentScope)
                {
                    yield return itemSet;
                }
            }
        }

        public class QualifiedName
        {
            public string ScopeSpecifier { get; private set; }
            public string UnqualifiedName { get; private set; }

            public QualifiedName(string name)
            {
                //default: the name has no specifier
                ScopeSpecifier = String.Empty;
                UnqualifiedName = name;
                //specifier is usually before the colon
                var parts = name.Split(new char[] {':'}, 2);
                if (parts.Length > 1 && ValidateScopeSpecifier(parts[0], false))
                {
                    ScopeSpecifier = parts[0];
                    UnqualifiedName = parts[1];
                }
            }
        }

        internal SessionStateCategory SessionStateCategory { get; private set; }

        public SessionState SessionState { get; private set; }
        public SessionStateScope<T> ParentScope{ get; private set; }
        public Dictionary<string, T> Items { get; private set; }
        public bool IsScriptScope { get { return SessionState.IsScriptScope; } }
        public IEnumerable<SessionStateScope<T>> HierarchyIterator
        {
            get { return new ScopeHierarchyIterator(this); }
        }


        public SessionStateScope(SessionState sessionState,SessionStateScope<T> parentItems,
                                 SessionStateCategory sessionStateCategory)
        {
            ParentScope = parentItems;
            Items = new Dictionary<string, T>(StringComparer.CurrentCultureIgnoreCase);
            //TODO: care about AllScope items!
            SessionState = sessionState;
            SessionStateCategory = sessionStateCategory;
        }

        #region general functions that work with full hierarchy and qualified names
        public T Get(string name, bool isQualified)
        {
            if (name == null)
            {
                throw new MethodInvocationException("The value of the argument \"name\" is null");
            }
            //if the name is qualified, look for the specified scope
            if (isQualified)
            {
                var qualName = new QualifiedName(name);
                if (!String.IsNullOrEmpty(qualName.ScopeSpecifier))
                {
                    SessionStateScope<T> affectedScope = GetScope(qualName.ScopeSpecifier, false);
                    var item = affectedScope.GetLocal(qualName.UnqualifiedName);
                    //return null if it's private
                    if (item == null || affectedScope != this && item.ItemOptions.HasFlag(ScopedItemOptions.Private))
                    {
                        return default(T);
                    }
                    return item;
                }
            }
            //no scope specifier in name, look in hierarchy
            var hostingScope = this.FindHostingScope(name);
            if (hostingScope != null)
            {
                return hostingScope.GetLocal(name);
            }
            return default(T);
        }

        public Dictionary<string, T> Find(string pattern, bool isQualified)
        {
            if (pattern == null)
            {
                throw new MethodInvocationException("The value of the argument \"pattern\" is null");
            }
            //if the name is qualified, look for the specified scope
            if (isQualified)
            {
                var qualName = new QualifiedName(pattern);
                if (!String.IsNullOrEmpty(qualName.ScopeSpecifier))
                {
                    SessionStateScope<T> affectedScope = GetScope(qualName.ScopeSpecifier, false);
                    var wildcard = new WildcardPattern(qualName.UnqualifiedName, WildcardOptions.IgnoreCase);
                    return (from pair in affectedScope.Items 
                           where wildcard.IsMatch(pair.Key) && 
                            (affectedScope == this || !pair.Value.ItemOptions.HasFlag(ScopedItemOptions.Private))
                                select pair.Value).ToDictionary(x => qualName.ScopeSpecifier + ":" + x.ItemName);
                }
            }
            var wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
            var visibleItems = new Dictionary<string, T>();
            //now check recursively the parent scopes for non-private, not overriden variables
            foreach (var curScope in new ScopeHierarchyIterator(this))
            {
                var matches = from pair in curScope.Items
                              where wildcardPattern.IsMatch(pair.Key) &&
                                    !visibleItems.ContainsKey(pair.Key) && 
                                    (!pair.Value.ItemOptions.HasFlag(ScopedItemOptions.Private) || curScope == this)
                              select pair.Value;
                foreach (var match in matches)
                {
                    visibleItems.Add(match.ItemName, match);
                }
            }
            return visibleItems;
        }

        public void Set(string name, T value, bool isQualified, bool overwrite)
        {
            if (name == null)
            {
                throw new MethodInvocationException("The value of the argument \"name\" is null");
            }
            var qualName = new QualifiedName(name);
            var isPrivate = isQualified ? String.Equals(ScopeSpecifiers.Private.ToString(), qualName.ScopeSpecifier,
                                                       StringComparison.CurrentCultureIgnoreCase)
                                        : false;
            var affectedScope = isQualified ? GetScope(qualName.ScopeSpecifier, false, this) : this;
            if (isPrivate) //make sure to set the private flag correctly
            {
                value.ItemOptions |= ScopedItemOptions.Private;
            }
            affectedScope.SetLocal(value, overwrite);
        }

        public void Remove(string name, bool isQualified)
        {
            if (name == null)
            {
                throw new MethodInvocationException("The value of the argument \"name\" is null");
            }
            //if the scope isn't specified, we will look in the hierarchy for the variable
            SessionStateScope<T> affectedScope = null;
            if (isQualified)
            {
                var qualName = new QualifiedName(name);
                affectedScope = GetScope(qualName.ScopeSpecifier, false);
                name = qualName.UnqualifiedName; //use the unqualified name to set the item
            }
            if (affectedScope == null)
            {
                affectedScope = FindHostingScope(name) ?? this;
            }
            affectedScope.RemoveLocal(name);
        }

        public Dictionary<string, T> GetAll()
        {
            //get a copy of the vars in the local scope first. Note: it also copies the correct comperator
            var visibleItems = new Dictionary<string, T>(Items);
            //now check recursively the parent scopes for non-private, not overriden variables
            //explicitly instantiate hierarchyiterator as ParentScope can be null
            foreach (var curScope in new ScopeHierarchyIterator(ParentScope))
            {
                foreach (var pair in curScope.Items)
                {
                    if (!visibleItems.ContainsKey(pair.Key) && 
                        !pair.Value.ItemOptions.HasFlag(ScopedItemOptions.Private))
                    {
                        visibleItems.Add(pair.Key, pair.Value);
                    }
                }
            }
            return visibleItems;
        }
        #endregion

        #region specified scope related

        public T GetAtScope(string name, string scope)
        {
            var affectedScope = GetScope(scope, true, this);
            return affectedScope.GetLocal(name);
        }

        public void SetAtScope(T value, string scope, bool overwrite)
        {
            var affectedScope = GetScope(scope, true, this);
            affectedScope.SetLocal(value, overwrite);
        }

        public void RemoveAtScope(string name, string scope)
        {
            var affectedScope = GetScope(scope, true, this);
            affectedScope.RemoveLocal(name);
        }

        public Dictionary<string, T> GetAllAtScope(string scope)
        {
            var affectedScope = GetScope(scope, true, this);
            return affectedScope.Items;
        }

        #endregion
           
        #region local scope only

        public bool HasLocal(string name)
        {
            return Items.ContainsKey(name);
        }

        public bool HasLocal(T item)
        {
            if (Items.ContainsKey(item.ItemName) && Items[item.ItemName].Equals(item))
            {
                return true;
            }
            return false;
        }

        public T GetLocal(string name)
        {
            if (name == null)
            {
                throw new MethodInvocationException("The value of the argument \"name\" is null");
            }
            if (Items.ContainsKey(name))
            {
                return Items[name];
            }
            return default(T);
        }

        public void SetLocal(T item, bool overwrite)
        {
            if (item == null)
            {
                throw new MethodInvocationException("The value of the argument \"item\" is null");
            }
            var original = GetLocal(item.ItemName);
            if (original != null)
            {
                if (!overwrite)
                {
                    throw new SessionStateException(item.ItemName, SessionStateCategory, String.Empty,
                                                    ErrorCategory.ResourceExists, null);
                }
                if (original.ItemOptions.HasFlag(ScopedItemOptions.ReadOnly))
                {
                    throw new SessionStateUnauthorizedAccessException(item.ItemName, SessionStateCategory,
                                                                      String.Empty, null);
                }
                if (!original.ItemOptions.HasFlag(ScopedItemOptions.Private)) //privacy cannot be changed
                {
                    item.ItemOptions &= ~ScopedItemOptions.Private;
                }
                else //original was private
                {
                    item.ItemOptions |= ScopedItemOptions.Private;
                }
                RemoveLocal(item.ItemName); //checks also for constants
            }
            Items.Add(item.ItemName, item);
        }

        public void RemoveLocal(string name)
        {
            if (name == null)
            {
                throw new MethodInvocationException("The value of the argument \"name\" is null");
            }
            var item = GetLocal(name);
            if (item == null) //doesn't exist
            {
                throw new ItemNotFoundException(name, SessionStateCategory, String.Empty, null);
            }
            if (item.ItemOptions.HasFlag(ScopedItemOptions.Constant))
            {
                throw new SessionStateUnauthorizedAccessException(name, SessionStateCategory, String.Empty, null);
            }
            Items.Remove(name);
        }

        #endregion

        #region public helper functions

        public SessionStateScope<T> FindHostingScope(string unqualifiedName)
        {
            //iterate through scopes and parents until we find the variable
            foreach (var candidate in HierarchyIterator)
            {
                var item = candidate.GetLocal(unqualifiedName);
                if (item == null)
                {
                    continue;
                }
                //make also sure the variable isn't private, if it's from a parent scope!
                if ((candidate == this) || !item.ItemOptions.HasFlag(ScopedItemOptions.Private))
                {
                    return candidate;
                }
            }
            return null; //nothing found
        }

        public SessionStateScope<T> GetScope(string specifier, bool numberAllowed,
                                             SessionStateScope<T> fallback = null)
        {
            int scopeLevel = -1;
            if (String.IsNullOrEmpty(specifier))
            {
                return fallback;
            }
            SessionStateScope<T> candidate = this;
            //check if the specifier is a number before trying to interprete it as enum
            if (int.TryParse(specifier, out scopeLevel))
            {
                //sometimes a number specifies the relative scope location, but in some contexts this isn't allowed
                //this has to happen *after* trying to parse them as e.g. "0" would be a parsable enum value!
                if (!numberAllowed || scopeLevel < 0)
                {
                    throw new ArgumentException("Invalid scope specifier");
                }
                for (var curLevel = scopeLevel; curLevel > 0; curLevel--)
                {
                    //make sure we can proceed looking for the next scope
                    candidate = candidate.ParentScope;
                    if (candidate == null)
                    {
                        throw new ArgumentOutOfRangeException("Exceeded the maximum number of available scopes");
                    }
                }
                return candidate;
            }
            //it's not a (positive) number, let's check if it's a named scope
            ScopeSpecifiers scopeSpecifier;
            if (!ScopeSpecifiers.TryParse(specifier, true, out scopeSpecifier))
            {
                throw new ArgumentException(String.Format("Invalid scope specifier \"{0}\".", specifier));
            }
            return GetScope(scopeSpecifier);
        }

        #endregion

        #region private helper functions

        private SessionStateScope<T> GetScope(ScopeSpecifiers specifier)
        {
            //if the local scope is meant, return this instance itself
            if (specifier == ScopeSpecifiers.Local || specifier == ScopeSpecifiers.Private)
            {
                return this;
            }
            var candidate = this;
            //otherwise it's a global or script scope specifier
            if (specifier == ScopeSpecifiers.Global || specifier == ScopeSpecifiers.Script)
            {
                while (candidate.ParentScope != null)
                {
                    if (candidate.IsScriptScope && specifier == ScopeSpecifiers.Script)
                    {
                        return candidate;
                    }
                    candidate = candidate.ParentScope;
                }
                //found the scope without a parent: the global scope
                return candidate;
            }
            throw new ArgumentException(String.Format("Invalid scope specifier \"{0}\"", specifier));
        }

        static private bool ValidateScopeSpecifier(string specifier, bool numberAllowed)
        {
            int scopeLevel = -1;
            //first try to interprete it as an int. if we don't do this, the enums TryParse method will misinterpret it
            if (int.TryParse(specifier, out scopeLevel) && (scopeLevel < 0 || !numberAllowed))
            {
                return false;
            }
            ScopeSpecifiers scopeSpecifier;
            if (!ScopeSpecifiers.TryParse(specifier, true, out scopeSpecifier))
            {
                return false;
            }
            return true;
        }

        #endregion                    
    }
}


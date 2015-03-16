// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using Pash.Implementation;
using System.Management.Automation.Provider;
using System.Collections.Generic;

namespace System.Management.Automation
{
    /// <summary>
    /// Handles child items in providers.
    /// </summary>
    public sealed class ChildItemCmdletProviderIntrinsics : CmdletProviderIntrinsicsBase
    {
        private ItemCmdletProviderIntrinsics Item
        {
            get
            {
                return new ItemCmdletProviderIntrinsics(InvokingCmdlet);
            }
        }

        internal ChildItemCmdletProviderIntrinsics(Cmdlet cmdlet) : base(cmdlet)
        {
        }

        internal ChildItemCmdletProviderIntrinsics(SessionState sessionState) : base(sessionState)
        {
        }


        #region Public API
        // Public API creating a default ProviderRuntime, calling the internal API and returning the results
        public Collection<PSObject> Get(string path, bool recurse)
        {
            return Get(new [] { path }, recurse, false, false);
        }

        public Collection<PSObject> Get(string[] path, bool recurse, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Get(path, recurse, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }

        public Collection<string> GetNames(string path, ReturnContainers returnContainers, bool recurse)
        {
            return GetNames(new [] { path }, returnContainers, recurse, false, false);
        }

        public Collection<string> GetNames(string[] path, ReturnContainers returnContainers, bool recurse, bool force,
                                           bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            GetNames(path, returnContainers, recurse, runtime);
            var packedResults = runtime.ThrowFirstErrorOrReturnResults();
            var results = (from r in packedResults select r.ToString()).ToList();
            return new Collection<string>(results);
        }

        public bool HasChild(string path)
        {
            return HasChild(path, false, false);
        }

        public bool HasChild(string path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            var res = HasChild(path, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return res;
        }

        #endregion

        #region internal API
        // actual work with callid the providers

        internal void Get(string[] paths, bool recurse, ProviderRuntime runtime)
        {
            // the include/exclude filters apply to the results, not to the globbing process. Make this sure
            runtime.IgnoreFiltersForGlobbing = true;

            // globbing is here a little more complicated, so we do it "manually" (without GlobAndInvoke)
            foreach (var curPath in paths)
            {
                var path = curPath;
                // if the path won't be globbed or filtered, we will directly list it's child
                var listChildsWithoutRecursion = !Globber.ShouldGlob(path, runtime) && !runtime.HasFilters();

                // the Path might be a mixture of a path and an include filter
                bool clearIncludeFilter;
                path = SplitFilterFromPath(path, recurse, runtime, out clearIncludeFilter);

                // now perform the actual globbing
                CmdletProvider provider;
                var globbed = Globber.GetGlobbedProviderPaths(path, runtime, out provider);
                var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
                var filter = new IncludeExcludeFilter(runtime.Include, runtime.Exclude, false);

                foreach (var globPath in globbed)
                {
                    try
                    {
                        // if we need to actively filter that stuff, we have to handle the recursion manually
                        if (!filter.CanBeIgnored)
                        {
                            ManuallyGetChildItems(containerProvider, globPath, recurse, filter, runtime);
                            return;
                        }
                        // otherwise just get the child items / the item directly
                        if (recurse || listChildsWithoutRecursion)
                        {
                            GetItemOrChildItems(containerProvider, globPath, recurse, runtime);
                            return;
                        }
                        // no recursion and globbing was performed: get the item, not the child items
                        containerProvider.GetItem(globPath, runtime);
                    }
                    catch (Exception e)
                    {
                        HandleCmdletProviderInvocationException(e);
                    }
                }
                // clean up the include filter of the runtime for the next item, if we split a filter from the path
                if (clearIncludeFilter)
                {
                    runtime.Include.Clear();
                }
            }
        }

        internal void GetNames(string[] path, ReturnContainers returnContainers, bool recurse, ProviderRuntime runtime)
        {
            // the include/exclude filters apply to the results, not to the globbing process. Make this sure
            runtime.IgnoreFiltersForGlobbing = true;
            // compile here, not in every recursive iteration
            var filter = new IncludeExcludeFilter(runtime.Include, runtime.Exclude, false);

            // do the globbing manually, because the behavior depends on it...
            foreach (var p in path)
            {
                CmdletProvider provider;
                var doGlob = Globber.ShouldGlob(p, runtime);
                // even if we don't actually glob, the next method will return the resolved path & provider
                var resolved = Globber.GetGlobbedProviderPaths(p, runtime, out provider);
                var contProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
                foreach (var curPath in resolved)
                {
                    if (!doGlob && filter.CanBeIgnored && !recurse)
                    {
                        contProvider.GetChildNames(curPath, returnContainers, runtime);
                        continue;
                    }
                    if ((recurse || !doGlob) && Item.IsContainer(contProvider, curPath, runtime))
                    {
                        ManuallyGetChildNames(contProvider, curPath, "", returnContainers, recurse, filter, runtime);
                        continue;
                    }
                    var cn = Path.ParseChildName(contProvider, curPath, runtime);
                    if (filter.Accepts(cn))
                    {
                        runtime.WriteObject(cn);
                    }
                }
            }
        }

        internal List<string> GetValidChildNames(ContainerCmdletProvider provider, string providerPath, ReturnContainers returnContainers, ProviderRuntime runtime)
        {
            var subRuntime = new ProviderRuntime(runtime);
            subRuntime.PassThru = false; // so we can catch the results
            provider.GetChildNames(providerPath, returnContainers, subRuntime);
            var results = subRuntime.ThrowFirstErrorOrReturnResults();
            return (from c in results
                where c.BaseObject is string
                select ((string)c.BaseObject)).ToList();
        }

        internal bool HasChild(string path, ProviderRuntime runtime)
        {
            var hasChild = false;
            GlobAndInvoke<ContainerCmdletProvider>(new [] { path }, runtime,
                (curPath, provider) => {
                    hasChild = hasChild || provider.HasChildItems(curPath, runtime);
                }
            );
            return hasChild;
        }

        #endregion

        #region private helpers

        string SplitFilterFromPath(string curPath, bool recurse, ProviderRuntime runtime, out bool clearIncludeFilter)
        {
            // When using Get-ChildItems, one could specify something like
            // "Get-ChildItem .\Source\*.cs -recurse" which should get all *.cs files recursively from .\Source.
            // This only works if no "-Include" was specified. So basically, it's a short form for
            // "Get-ChildItem .\Source -recurse -include *.cs", but of course the first is easier to write :)
            clearIncludeFilter = false;
            // first check if we deal with a LiteralPath, a container, or include is set. If so, this won't work
            // also if we don't need to recurse, the globber can simply do the work
            if (!recurse ||
                !Globber.ShouldGlob(curPath, runtime) ||
                (runtime.Include != null && runtime.Include.Count > 0) ||
                Item.IsContainer(curPath, runtime))
            {
                return curPath;
            }
            clearIncludeFilter = true;
            var childName = Path.ParseChildName(curPath, runtime);
            runtime.Include.Add(childName);
            return curPath.Substring(0, curPath.Length - childName.Length);
        }

        private void GetItemOrChildItems(ContainerCmdletProvider provider, string path, bool recurse, ProviderRuntime runtime)
        {
            // If path identifies a container it gets all child items (recursively or not). Otherwise it returns the leaf
            if (Item.IsContainer(provider, path, runtime))
            {
                provider.GetChildItems(path, recurse, runtime);
                return;
            }
            provider.GetItem(path, runtime);
        }

        private void ManuallyGetChildItems(ContainerCmdletProvider provider, string path, bool recurse, 
                                           IncludeExcludeFilter filter, ProviderRuntime runtime)
        {
            // recursively get child names of containers or just the current child if the filter accepts it
            if (recurse && Item.IsContainer(path, runtime))
            {
                ManuallyGetChildItemsFromContainer(provider, path, recurse, filter, runtime);
                return;
            }
            var childName = Path.ParseChildName(provider, path, runtime);
            if (filter.Accepts(childName))
            {
                provider.GetItem(path, runtime);
            }
        }

        private void ManuallyGetChildItemsFromContainer(ContainerCmdletProvider provider, string path, bool recurse, 
                                                        IncludeExcludeFilter filter, ProviderRuntime runtime)
        {
            // we deal with a container: get all child items (all containers if we recurse)
            Dictionary<string, bool> matches = null;
            // When a provider specific filter is set, and we need to recurse, we need to check recurse into all
            // containers, but just get those that match the internal filter. Therefore we construct a lookup dict.
            // Looking up in a dictionary whether or not the itemis a match should be faster than using a list
            // If there is no filter, then ReturnAllContainers and ReturnMatchingContainers don't differ
            if (!String.IsNullOrEmpty(runtime.Filter))
            {
                matches = GetValidChildNames(provider, path, ReturnContainers.ReturnMatchingContainers,
                                             runtime).ToDictionary(c => c, c => true);
            }
            var childNames = GetValidChildNames(provider, path, ReturnContainers.ReturnAllContainers, runtime);
            foreach (var childName in childNames)
            {
                var childPath = Path.Combine(provider, path, childName, runtime);
                // if the filter accepts the child (leaf or container) and it's potentially a filter match, get it
                if (filter.Accepts(childName) && (matches == null || matches.ContainsKey(childName)))
                {
                    provider.GetItem(childPath, runtime);
                }
                // if we need to recurse and deal with a container, dive into it
                if (recurse && Item.IsContainer(childPath, runtime))
                {
                    ManuallyGetChildItemsFromContainer(provider, childPath, true, filter, runtime);
                }
            }
        }

        private void ManuallyGetChildNames(ContainerCmdletProvider provider, string providerPath, string relativePath,
            ReturnContainers returnContainers, bool recurse, IncludeExcludeFilter filter, ProviderRuntime runtime)
        {
            // Affected by #trailingSeparatorAmbiguity
            // Sometimes, PS removes or appends a trailing slash to the providerPath
            // E.g. when the recurse == true, there is a trailing slash, but not when recurse == false.
            // As it calls the method with the slash being appended and not appended, PS doesn't seem to make
            // promises to the provider implementation whether or not the path has a trailing slash
            var childNames = GetValidChildNames(provider, providerPath, returnContainers, runtime);
            foreach (var childName in childNames)
            {
                // add the child only if the filter accepts it
                if (!filter.Accepts(childName))
                {
                    continue;
                }
                var path = Path.Combine(provider, relativePath, childName, runtime);
                runtime.WriteObject(path);
            }
            // check if we need to handle this recursively
            if (!recurse)
            {
                return;
            }
            // okay, we should use recursion, so get all child containers and call this function again
            childNames = GetValidChildNames(provider, providerPath, ReturnContainers.ReturnAllContainers, runtime);
            foreach (var childName in childNames)
            {
                var providerChildPath = Path.Combine(provider, providerPath, childName, runtime);
                if (Item.IsContainer(providerChildPath, runtime))
                {
                    // recursive call wirth child's provider path and relative path
                    var relativeChildPath = Path.Combine(provider, relativePath, childName, runtime);
                    ManuallyGetChildNames(provider, providerChildPath, relativeChildPath, returnContainers,
                        true, filter, runtime);
                }
            }
        }

        #endregion
    }
}

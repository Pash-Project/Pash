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
            throw new NotImplementedException();
        }

        public bool HasChild(string path, bool force, bool literalPath)
        {
            throw new NotImplementedException();
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
                path = SplitFilterFromPath(path, runtime, out clearIncludeFilter);

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

            GlobAndInvoke<ContainerCmdletProvider>(path, runtime,
                (curPath, Provider) => ManuallyGetChildNames(Provider, curPath, returnContainers, recurse, filter, runtime)
            );
        }

        internal string GetChildName(string path, ProviderRuntime runtime)
        {
            ProviderInfo info;
            path = Globber.GetProviderSpecificPath(path, runtime, out info);
            var provider = SessionState.Provider.GetInstance(info) as NavigationCmdletProvider;
            return provider == null ? path : provider.GetChildName(path, runtime);
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

        #endregion

        #region private helpers

        string SplitFilterFromPath(string curPath, ProviderRuntime runtime, out bool clearIncludeFilter)
        {
            // When using Get-ChildItems, one could specify something like
            // "Get-ChildItem .\Source\*.cs -recurse" which should get all *.cs files recursively from .\Source.
            // This only works if no "-Include" was specified. So basically, it's a short form for
            // "Get-ChildItem .\Source -recurse -include *.cs", but of course the first is easier to write :)
            clearIncludeFilter = false;
            // first check if we deal with a LiteralPath, a container, or include is set. If so, this won't work
            if (!Globber.ShouldGlob(curPath, runtime) ||
                (runtime.Include != null && runtime.Include.Count > 0) ||
                Item.IsContainer(curPath, runtime))
            {
                return curPath;
            }
            clearIncludeFilter = true;
            var childName = GetChildName(curPath, runtime);
            runtime.Include.Add(childName);
            return curPath.Substring(0, curPath.Length - childName.Length);
        }

        private void GetItemOrChildItems(ContainerCmdletProvider provider, string path, bool recurse, ProviderRuntime runtime)
        {
            // If path identifies a container it gets all child items (recursively or not). Otherwise it returns the leaf
            if (Item.IsContainer(path, runtime))
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
            var childName = GetChildName(path, runtime);
            if (filter.Accepts(childName))
            {
                provider.GetItem(path, runtime);
            }
        }

        private void ManuallyGetChildItemsFromContainer(ContainerCmdletProvider provider, string path, bool recurse, 
                                                        IncludeExcludeFilter filter, ProviderRuntime runtime)
        {
            // we deal with a container: get all child items (all containers if we recurse)
            var childNames = GetValidChildNames(provider, path, ReturnContainers.ReturnMatchingContainers, runtime);
            foreach (var childName in childNames)
            {
                // if the filter accepts the child (leaf or container), get it
                if (filter.Accepts(childName))
                {
                    var childPath = Path.Combine(provider, path, childName, runtime);
                    provider.GetItem(childPath, runtime);
                }
            }
            // check for recursion
            if (!recurse)
            {
                return;
            }
            // we are in recursion: dive into child containers
            childNames = GetValidChildNames(provider, path, ReturnContainers.ReturnAllContainers, runtime);
            foreach (var childName in childNames)
            {
                var childPath = Path.Combine(provider, path, childName, runtime);
                if (Item.IsContainer(childPath, runtime))
                {
                    ManuallyGetChildItemsFromContainer(provider, childPath, true, filter, runtime);
                }
            }
        }

        private void ManuallyGetChildNames(ContainerCmdletProvider provider, string providerPath,
            ReturnContainers returnContainers, bool recurse, IncludeExcludeFilter filter, ProviderRuntime runtime)
        {
            // this function doesn't use a globber, it expects a finished provider path. But it can invoke recursion
            if (Item.IsContainer(providerPath, runtime))
            {
                ManuallyGetChildNamesFromContainer(provider, providerPath, "", returnContainers, recurse, filter, runtime);
                return;
            }
            var childName = GetChildName(providerPath, runtime);
            if (filter.Accepts(childName))
            {
                runtime.WriteObject(childName);
            }
        }

        private void ManuallyGetChildNamesFromContainer(ContainerCmdletProvider provider, string providerPath, string relativePath,
            ReturnContainers returnContainers, bool recurse, IncludeExcludeFilter filter, ProviderRuntime runtime)
        {
            // we deal with a container
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
                    ManuallyGetChildNamesFromContainer(provider, providerChildPath, relativeChildPath, returnContainers,
                        true, filter, runtime);
                }
            }
        }

        #endregion
    }
}

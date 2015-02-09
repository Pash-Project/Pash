// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using Pash.Implementation;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    /// <summary>
    /// Handles child items in providers.
    /// </summary>
    public sealed class ChildItemCmdletProviderIntrinsics : CmdletProviderIntrinsicsBase
    {
        private ItemCmdletProviderIntrinsics Item { get; set; }

        internal ChildItemCmdletProviderIntrinsics(Cmdlet cmdlet) : base(cmdlet)
        {
            Item = new ItemCmdletProviderIntrinsics(cmdlet);
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

        internal void Get(string[] path, bool recurse, ProviderRuntime runtime)
        {
            // the include/exclude filters apply to the results, not to the globbing process. Make this sure
            runtime.IgnoreFiltersForGlobbing = true;

        }

        internal void GetNames(string[] path, ReturnContainers returnContainers, bool recurse, ProviderRuntime runtime)
        {
            // the include/exclude filters apply to the results, not to the globbing process. Make this sure
            runtime.IgnoreFiltersForGlobbing = true;
            // compile here, not in every recursive iteration
            var filter = new IncludeExcludeFilter(runtime.Include, runtime.Exclude, false);

            GlobAndInvoke<ContainerCmdletProvider>(path, runtime,
                (curPath, Provider) => GetNamesRecursively(Provider, curPath, "", returnContainers, recurse, runtime, filter)
            );
        }

        #endregion

        #region private helpers

        private void GetNamesRecursively(ContainerCmdletProvider provider, string providerPath, string relativePath,
            ReturnContainers returnContainers, bool recurse, ProviderRuntime runtime, IncludeExcludeFilter filter)
        {
            // this function doesn't use a globber, it expects a finished provider path. But it uses recursion
            var subRuntime = new ProviderRuntime(runtime);
            subRuntime.PassThru = false;
            // get child names for the current providerPath
            provider.GetChildNames(providerPath, returnContainers, subRuntime);
            var childNames = subRuntime.ThrowFirstErrorOrReturnResults();
            foreach (var childNameObj in childNames)
            {
                var childName = childNameObj.BaseObject as string;
                if (childName == null)
                {
                    continue;
                }
                // add the child only if the filter accepts it
                if (!filter.Accepts(childName))
                {
                    continue;
                }
                var path = Path.Combine(provider, relativePath, childName, runtime);
                runtime.WriteObject(path);
            }

            // now check if we need to handle this recursively
            if (!recurse)
            {
                return;
            }
            // okay, we should use recursion, so get all child containers and call this function again
            provider.GetChildNames(providerPath, ReturnContainers.ReturnAllContainers, subRuntime);
            childNames = subRuntime.ThrowFirstErrorOrReturnResults();
            foreach (var containerChild in childNames)
            {
                var childName = containerChild.BaseObject as string;
                if (childName == null)
                {
                    continue;
                }
                var providerChildPath = Path.Combine(provider, providerPath, childName, runtime);
                if (Item.IsContainer(providerChildPath, runtime))
                {
                    // recursive call wirth child's provider path and relative path
                    var relativeChildPath = Path.Combine(provider, relativePath, childName, runtime);
                    GetNamesRecursively(provider, providerChildPath, relativeChildPath, returnContainers, true, runtime, filter);
                }
            }
        }

        #endregion
    }
}

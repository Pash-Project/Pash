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

        internal void Get(string[] path, bool recurse, ProviderRuntime providerRuntime)
        {
            throw new NotImplementedException();
        }

        internal void GetNames(string[] path, ReturnContainers returnContainers, bool recurse, ProviderRuntime runtime)
        {
            GlobAndInvoke<ContainerCmdletProvider>(path, runtime,
                (curPath, Provider) => {
                    GetChildNamesFromProviderPath(Provider, curPath, "", returnContainers, recurse, runtime);
                }
            );
        }

        #endregion

        #region private helpers

        private void GetChildNamesFromProviderPath(ContainerCmdletProvider provider, string providerPath, string relativePath,
            ReturnContainers returnContainers, bool recurse, ProviderRuntime runtime)
        {
            var subRuntime = new ProviderRuntime(runtime);
            subRuntime.PassThru = false;
            provider.GetChildNames(providerPath, returnContainers, subRuntime);
            var childNames = subRuntime.ThrowFirstErrorOrReturnResults();
            foreach (var childNameObj in childNames)
            {
                var childName = childNameObj.BaseObject as string;
                if (childName == null)
                {
                    continue;
                }
                // TODO: check runtime's include/exclude filters
                var path = Path.Combine(provider, relativePath, childName, runtime);
                runtime.WriteObject(path);
            }
            // now check if we need to handle this recursively
            if (!recurse)
            {
                return;
            }
            // this is recursion, so get all containers from item
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
                    GetChildNamesFromProviderPath(provider, providerChildPath, relativeChildPath, returnContainers, true, runtime);
                }
            }
        }

        #endregion
    }
}

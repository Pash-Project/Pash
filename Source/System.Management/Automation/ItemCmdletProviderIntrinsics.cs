// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class ItemCmdletProviderIntrinsics : CmdletProviderIntrinsicsBase
    {
        private ChildItemCmdletProviderIntrinsics ChildItem
        {
            get
            {
                return new ChildItemCmdletProviderIntrinsics(InvokingCmdlet);
            }
        }

        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet) : base(cmdlet)
        {
        }

        internal ItemCmdletProviderIntrinsics(SessionState sessionState) : base(sessionState)
        {
        }

        #region Public API
        // They work without the use of ProviderRuntime. All these functions basically create a ProviderRuntime,
        // call the internal function, check for errors and return the result of the ProviderRuntime's result buffer

        public Collection<PSObject> Clear(string path)
        {
            return Clear(new [] { path }, false, false);
        }

        public Collection<PSObject> Clear(string[] path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Clear(path, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }

        public Collection<PSObject> Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers)
        {
            return Copy(new [] { path }, destinationPath, recurse, copyContainers, false, false);
        }

        public Collection<PSObject> Copy(string[] path, string destinationPath, bool recurse, CopyContainers copyContainers,
                                         bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Copy(path, destinationPath, recurse, copyContainers, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }

        public bool Exists(string path)
        {
            return Exists(path, false, false);
        }

        public bool Exists(string path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            bool result = Exists(path, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return result;
        }

        public Collection<PSObject> Get(string path)
        {
            return Get(new [] { path }, false, false);
        }

        public Collection<PSObject> Get(string[] path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Get(path, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }

        public void Invoke(string path)
        {
            Invoke(new [] { path }, false);
        }

        public void Invoke(string[] path, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState);
            runtime.AvoidGlobbing = literalPath;
            Invoke(path, runtime);
            runtime.ThrowFirstErrorOrContinue();
        }

        public bool IsContainer(string path)
        {
            var runtime = new ProviderRuntime(SessionState);
            bool result = IsContainer(path, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return result;
        }

        public Collection<PSObject> Move(string path, string destination)
        {
            return Move(new [] { path }, destination, false, false);
        }

        public Collection<PSObject> Move(string[] path, string destination, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Move(path, destination, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }


        public Collection<PSObject> New(string path, string name, string itemTypeName, object content)
        {
            return New(new [] { path }, name, itemTypeName, content, false);
        }

        public Collection<PSObject> New(string[] paths, string name, string itemTypeName, object content, bool force)
        {
            var runtime = new ProviderRuntime(SessionState, force, true);
            New(paths, name, itemTypeName, content, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }

        public void Remove(string path, bool recurse)
        {
            Remove(new [] { path }, recurse, false, false);
        }

        public void Remove(string[] path, bool recurse, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Remove(path, recurse, runtime);
            runtime.ThrowFirstErrorOrContinue();
        }

        public Collection<PSObject> Rename(string path, string newName)
        {
            return Rename(path, newName, false);
        }

        public Collection<PSObject> Rename(string path, string newName, bool force)
        {
            var runtime = new ProviderRuntime(SessionState, force, true);
            Rename(path, newName, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }

        public Collection<PSObject> Set(string path, object value)
        {
            return Set(new [] { path }, value, false, false);
        }

        public Collection<PSObject> Set(string[] path, object value, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Set(path, value, runtime);
            return runtime.ThrowFirstErrorOrReturnResults();
        }


        #endregion

        #region internal API

        // these function calls use directly a ProviderRuntime and don't return objects, but write it to the
        // ProviderRuntime's result buffer (which might be the cmdlet's result buffer)

        internal void Clear(string[] path, ProviderRuntime runtime)
        {
            GlobAndInvoke<ItemCmdletProvider>(path, runtime,
                (curPath, provider) => provider.ClearItem(curPath, runtime)
            );
        }

        internal object ClearItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Copy(string[] path, string destinationPath, bool recurse, CopyContainers copyContainers, ProviderRuntime runtime)
        {
            ProviderInfo destinationProvider;
            var destination = Globber.GetProviderSpecificPath(destinationPath, runtime, out destinationProvider);
            // make sure we don't use the version of IsContainer that globs, or we will have unnecessary provider callbacks
            var destProvider = destinationProvider.CreateInstance() as ContainerCmdletProvider; // it's okay to be null
            var destIsContainer = IsContainer(destProvider, destination, runtime);
            GlobAndInvoke<ContainerCmdletProvider>(path, runtime,
                (curPath, provider) => {
                    if (!runtime.PSDriveInfo.Provider.Equals(destinationProvider))
                    {
                        var msg = "The source cannot be copied to the destination, because they're not from the same provider";
                        var error = new PSArgumentException(msg, "CopyItemSourceAndDestinationNotSameProvider",
                            ErrorCategory.InvalidArgument);
                        runtime.WriteError(error.ErrorRecord);
                        return;
                    }
                    // Affected by #trailingSeparatorAmbiguity
                    // PS would make sure the trailing slash of curPath is removed
                    // check if src is a container
                    if (IsContainer(provider, curPath, runtime))
                    {
                        // if we copy a container to another, invoke a special method for this
                        if (destIsContainer)
                        {
                            CopyContainerToContainer(provider, curPath, destination, recurse, copyContainers, runtime);
                            return;
                        }
                        // otherwise the destination doesn't exist or is a leaf. Copying a container to a leaf doesn't work
                        if (Exists(destination, runtime))
                        {
                            var error = new PSArgumentException("Cannot copy container to existing leaf", 
                                "CopyContainerItemToLeafError", ErrorCategory.InvalidArgument).ErrorRecord;
                            runtime.WriteError(error);
                            return;
                        }
                        // otherwise we just proceed as normal
                    }
                    // either leaf to leaf, leaf to container, or container to not-existing (i.e. copy the container)
                    provider.CopyItem(curPath, destination, recurse, runtime);
                }
            );
        }

        internal object CopyItemDynamicParameters(string[] path, string destination, bool recurse, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal bool Exists(string path, ProviderRuntime runtime)
        {
            CmdletProvider provider;
            var globbedPaths = Globber.GetGlobbedProviderPaths(path, runtime, false, out provider);
            var itemProvider = provider as ItemCmdletProvider;
            // we assume that in a low level CmdletProvider all items exists. Not sure about this, but I don't want to
            // break existing functionality
            if (itemProvider == null)
            {
                return true;
            }
            foreach (var p in globbedPaths)
            {
                var exists = false;
                try
                {
                    exists = itemProvider.ItemExists(p, runtime);
                }
                catch (Exception e)
                {
                    HandleCmdletProviderInvocationException(e);
                }
                if (exists)
                {
                    return true;
                }
            }
            return false;
        }

        internal void Get(string[] path, ProviderRuntime runtime)
        {
            GlobAndInvoke<ItemCmdletProvider>(path, runtime,
                (curPath, provider) => {
                    provider.GetItem(curPath, runtime);
                }
            );
        }

        internal object GetItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Invoke(string[] path, ProviderRuntime runtime)
        {
            GlobAndInvoke<ItemCmdletProvider>(path, runtime,
                (curPath, provider) => provider.InvokeDefaultAction(curPath, runtime)
            );
        }

        internal object InvokeItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal bool IsContainer(string path, ProviderRuntime runtime)
        {
            CmdletProvider provider;
            var globbedPaths = Globber.GetGlobbedProviderPaths(path, runtime, false, out provider);
            var containerProvider = provider as ContainerCmdletProvider;
            var isNavProvider = provider is NavigationCmdletProvider;
            if (containerProvider == null && !isNavProvider)
            {
                throw new NotSupportedException("The affected provider doesn't support container related operations.");
            }
            foreach (var p in globbedPaths)
            {
                if (!IsContainer(containerProvider, p, runtime))
                {
                    return false;
                }
            }
            // all globbed paths are containers
            return true;
        }

        internal bool IsContainer(ContainerCmdletProvider provider, string path, ProviderRuntime runtime)
        {
            var navProvider = provider as NavigationCmdletProvider;
            // path is an expanded path to a single location. no globbing and filtering is performed
            if (navProvider != null)
            {
                return navProvider.IsItemContainer(path, runtime);
            }
            // otherwise it's just a ContainerCmdletProvider. It doesn't support hierarchies,
            // only drives can be containers
            // an empty path means "root" path in a drive
            return path.Length == 0 || path.Equals(runtime.PSDriveInfo.Root);
        }

        internal object ItemExistsDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Move(string[] path, string destinationPath, ProviderRuntime runtime)
        {
            ProviderInfo destinationProvider;
            var destination = Globber.GetProviderSpecificPath(destinationPath, runtime, out destinationProvider);
            GlobAndInvoke<NavigationCmdletProvider>(path, runtime,
                (curPath, provider) => {
                    // TODO: I think Powershell checks whether we are currently in the path we want to remove
                    //       (or a subpath). Check this and throw an error if it's true
                    if (!runtime.PSDriveInfo.Provider.Equals(destinationProvider))
                    {
                        var msg = "The source cannot be moved to the destination, because they're not from the same provider";
                        var error = new PSArgumentException(msg, "MoveItemSourceAndDestinationNotSameProvider",
                            ErrorCategory.InvalidArgument);
                        runtime.WriteError(error.ErrorRecord);
                        return;
                    }
                    provider.MoveItem(curPath, destination, runtime);
                }
            );
        }

        internal object MoveItemDynamicParameters(string path, string destination, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void New(string[] paths, string name, string type, object content, ProviderRuntime runtime)
        {
            var validName = !String.IsNullOrEmpty(name);
            CmdletProvider provider;
            foreach (var path in paths)
            {
                Collection<string> resolvedPaths;
                // only allow globbing if name is used. otherwise it doesn't make sense
                if (validName)
                {
                    resolvedPaths = Globber.GetGlobbedProviderPaths(path, runtime, false, out provider);
                }
                else
                {
                    ProviderInfo providerInfo;
                    resolvedPaths = new Collection<string>();
                    resolvedPaths.Add(Globber.GetProviderSpecificPath(path, runtime, out providerInfo));
                    provider = SessionState.Provider.GetInstance(providerInfo);
                }
                var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
                foreach (var curResolvedPath in resolvedPaths)
                {
                    var resPath = curResolvedPath;
                    if (validName)
                    {
                        resPath = Path.Combine(containerProvider, resPath, name, runtime);
                    }
                    try
                    {
                        containerProvider.NewItem(resPath, type, content, runtime);
                    }
                    catch (Exception e)
                    {
                        HandleCmdletProviderInvocationException(e);
                    }
                }
            }
        }

        internal object NewItemDynamicParameters(string path, string type, object content, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Remove(string[] path, bool recurse, ProviderRuntime runtime)
        {
            GlobAndInvoke<ContainerCmdletProvider>(path, runtime,
                (curPath, provider) => {
                    // TODO: I think Powershell checks whether we are currently in the path we want to remove
                    //       (or a subpath). Check this and throw an error if it's true
                    if (provider.HasChildItems(curPath, runtime) && !recurse)
                    {
                        // TODO: I think Powershell invokes ShouldContinue here and asks whether to remove
                        //       items recursively or not. We should somehow do this too. Maybe by getting
                        //       access to runtime._cmdlet, or by implementing a wrapper function in ProviderRuntime
                        var msg = String.Format("The item at path '{0}' has child items. Use recursion to remove it",
                            curPath);
                        var invOpEx = new PSInvalidOperationException(msg, "CannotRemoveItemWithChildrenWithoutRecursion",
                            ErrorCategory.InvalidOperation, null);
                        // FIXME: In this case, Powershell does throw a CmdletInvocationException. Maybe because
                        //        this check is done directly inside the Remove-Item cmdlet, or maybe it only
                        //        happens if ShouldContinue doesn't work in a non-interactive environment.
                        //        Anyway, it feels right that the work is done here and we will simply throw this
                        //        kind of exception for compatability. Maybe when the TODO before is approach we should
                        //        keep in mind that this kind of exception is required to be thrown
                        throw new CmdletInvocationException(invOpEx.Message, invOpEx);
                    }
                    provider.RemoveItem(curPath, recurse, runtime);
                }
            );
        }

        internal object RemoveItemDynamicParameters(string path, bool recurse, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Rename(string path, string newName, ProviderRuntime runtime)
        {
            CmdletProvider provider;
            var globbed = Globber.GetGlobbedProviderPaths(path, runtime, out provider);
            if (globbed.Count != 1)
            {
                throw new PSArgumentException("Cannot rename more than one item", "MultipleItemsRename", ErrorCategory.InvalidArgument);
            }
            path = globbed[0];
            var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
            // TODO: I think Powershell checks whether we are currently in the path we want to remove
            //       (or a subpath). Check this and throw an error if it's true
            try
            {
                containerProvider.RenameItem(path, newName, runtime);
            }
            catch (Exception e)
            {
                HandleCmdletProviderInvocationException(e);
            }
        }

        internal object RenameItemDynamicParameters(string path, string newName, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Set(string[] path, object value, ProviderRuntime runtime)
        {
            GlobAndInvoke<ItemCmdletProvider>(path, runtime,
                (curPath, provider) => provider.SetItem(curPath, value, runtime)
            );
        }

        internal object SetItemDynamicParameters(string path, object value, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region private helpers

        void CopyContainerToContainer(ContainerCmdletProvider provider, string srcPath, string destPath, bool recurse,
                                      CopyContainers copyContainers, ProviderRuntime runtime)
        {
            // the "usual" case: if we don't use recursion (empty container is copied) or we want to maintain the
            // original hierarchy
            if (!recurse || copyContainers.Equals(CopyContainers.CopyTargetContainer))
            {
                provider.CopyItem(srcPath, destPath, recurse, runtime);
                return;
            }
            // Otherwise we want a flat-hierachy copy of a folder (because copyContainers is CopyChildrenOfTargetContainer)
            // Make sure recurse is set
            if (!recurse)
            {
                var error = new PSArgumentException("Cannot copy container to existing leaf",
                    "CopyContainerItemToLeafError", ErrorCategory.InvalidArgument).ErrorRecord;
                runtime.WriteError(error);
                return;
            }
            // otherwise do the flat copy. To do this: get all child names (recursively) and invoke copying without recursion
            var childNames = ChildItem.GetNames(srcPath, ReturnContainers.ReturnMatchingContainers, true);
            foreach (var child in childNames)
            {
                var childPath = Path.Combine(provider, srcPath, child, runtime);
                provider.CopyItem(childPath, destPath, false, runtime);
            }
        }
        #endregion
    }
}

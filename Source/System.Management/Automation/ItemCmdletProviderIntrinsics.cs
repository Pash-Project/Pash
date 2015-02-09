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
        private ChildItemCmdletProviderIntrinsics ChildItem { get; set; }

        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet) : base(cmdlet)
        {
            ChildItem = new ChildItemCmdletProviderIntrinsics(cmdlet);
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
            runtime.AvoidWildcardExpansion = literalPath;
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
            ProviderInfo providerInfo;
            PSDriveInfo driveInfo;
            var globber = new PathGlobber(ExecutionContext.SessionState);
            var destination = globber.GetProviderSpecificPath(destinationPath, runtime, out providerInfo);
            var destIsContainer = IsContainer(destination, runtime);

            GlobAndInvoke<ContainerCmdletProvider>(path, runtime,
                (curPath, provider) => {
                    // make sure the src exists
                    if(!VerifyItemExists(provider, curPath, runtime))
                    {
                        return;
                    }
                    // check if src is a container
                    if (IsContainer(curPath, runtime))
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
            var globber = new PathGlobber(ExecutionContext.SessionState);
            var globbedPaths = globber.GetGlobbedProviderPaths(path, runtime, out provider);
            var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
            foreach (var p in globbedPaths)
            {
                var exists = false;
                try
                {
                    exists = containerProvider.ItemExists(p, runtime);
                }
                catch (ItemNotFoundException e)
                {
                    return false;
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
                    if (VerifyItemExists(provider, curPath, runtime))
                    {
                        provider.GetItem(curPath, runtime);
                    }
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
            var globber = new PathGlobber(ExecutionContext.SessionState);
            var globbedPaths = globber.GetGlobbedProviderPaths(path, runtime, out provider);
            var isContainerProvider = (provider is ContainerCmdletProvider);
            var navProvider = provider as NavigationCmdletProvider;
            if (!isContainerProvider && navProvider == null)
            {
                throw new NotSupportedException("The affected provider doesn't support container related operations.");
            }
            foreach (var p in globbedPaths)
            {
                if (navProvider != null)
                {
                    throw new NotImplementedException("No support for provider specific container check, yet.");
                }
                // otherwise it's just a ContainerCmdletProvider. It doesn't support hierarchies, only drives can be containers
                // an empty path means "root" path in a drive
                if (p.Length > 0)
                {
                    // path isn't a drives root path
                    return false;
                }
            }
            // all globbed paths are containers
            return true;
        }

        internal object ItemExistsDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Move(string[] path, string destination, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object MoveItemDynamicParameters(string path, string destination, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void New(string[] paths, string name, string type, object content, ProviderRuntime runtime)
        {
            var validName = !String.IsNullOrEmpty(name);
            CmdletProvider provider;
            var globber = new PathGlobber(ExecutionContext.SessionState);
            foreach (var path in paths)
            {
                Collection<string> resolvedPaths;
                // only allow globbing if name is used. otherwise it doesn't make sense
                if (validName)
                {
                    resolvedPaths = globber.GetGlobbedProviderPaths(path, runtime, out provider);
                }
                else
                {
                    ProviderInfo providerInfo;
                    resolvedPaths = new Collection<string>();
                    resolvedPaths.Add(globber.GetProviderSpecificPath(path, runtime, out providerInfo));
                    provider = ExecutionContext.SessionState.Provider.GetInstance(providerInfo);
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
                    if (!VerifyItemExists(provider, curPath, runtime))
                    {
                        return;
                    }
                    // TODO: I think Powershell checks whether we are currently in the path we want to remove
                    //       (or a subpath). Check this and throw an error if it's true
                    if (!recurse && provider.HasChildItems(curPath, runtime))
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
            var globber = new PathGlobber(ExecutionContext.SessionState);
            var globbed = globber.GetGlobbedProviderPaths(path, runtime, out provider);
            if (globbed.Count != 1)
            {
                throw new PSArgumentException("Cannot rename more than one item", "MultipleItemsRename", ErrorCategory.InvalidArgument);
            }
            path = globbed[0];
            var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
            if (!VerifyItemExists(containerProvider, path, runtime))
            {
                return;
            }
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

        private bool VerifyItemExists(ItemCmdletProvider provider, string path, ProviderRuntime runtime)
        {
            var exists = false;
            try
            {
                exists = provider.ItemExists(path, runtime);
            }
            catch (Exception e)
            {
                HandleCmdletProviderInvocationException(e);
            }

            if (exists)
            {
                return true;
            }
            var msg = String.Format("An item with path {0} doesn't exist", path);
            runtime.WriteError(new ItemNotFoundException(msg).ErrorRecord);
            return false;
        }

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

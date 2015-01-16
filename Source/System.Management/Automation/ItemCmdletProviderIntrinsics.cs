// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using System.IO;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class ItemCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        private ExecutionContext _executionContext;

        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet) : this(cmdlet.ExecutionContext)
        {
            _cmdlet = cmdlet;
        }

        internal ItemCmdletProviderIntrinsics(ExecutionContext context)
        {
            _executionContext = context;
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
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
            Clear(path, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }

        public Collection<PSObject> Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers)
        {
            return Copy(new [] { path }, destinationPath, recurse, copyContainers, false, false);
        }

        public Collection<PSObject> Copy(string[] path, string destinationPath, bool recurse, CopyContainers copyContainers,
                                         bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
            Copy(path, destinationPath, recurse, copyContainers, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }

        public bool Exists(string path)
        {
            return Exists(path, false, false);
        }

        public bool Exists(string path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
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
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
            Get(path, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }

        public void Invoke(string path)
        {
            Invoke(new [] { path }, false);
        }

        public void Invoke(string[] path, bool literalPath)
        {
            var runtime = new ProviderRuntime(_executionContext);
            Invoke(path, literalPath, runtime);
            runtime.ThrowFirstErrorOrContinue();
        }

        public bool IsContainer(string path)
        {
            var runtime = new ProviderRuntime(_executionContext);
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
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
            Move(path, destination, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }


        public Collection<PSObject> New(string path, string name, string itemTypeName, object content)
        {
            return New(new [] { path }, name, itemTypeName, content, false);
        }

        public Collection<PSObject> New(string[] paths, string name, string itemTypeName, object content, bool force)
        {
            var runtime = new ProviderRuntime(_executionContext, force, true);
            New(paths, name, itemTypeName, content, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }

        public void Remove(string path, bool recurse)
        {
            Remove(new [] { path }, recurse, false, false);
        }

        public void Remove(string[] path, bool recurse, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
            Remove(path, recurse, runtime);
            runtime.ThrowFirstErrorOrContinue();
        }

        public Collection<PSObject> Rename(string path, string newName)
        {
            return Rename(path, newName, false);
        }

        public Collection<PSObject> Rename(string path, string newName, bool force)
        {
            var runtime = new ProviderRuntime(_executionContext, force, true);
            Rename(path, newName, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }

        public Collection<PSObject> Set(string path, object value)
        {
            return Set(new [] { path }, value, false, false);
        }

        public Collection<PSObject> Set(string[] path, object value, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(_executionContext, force, literalPath);
            Set(path, value, runtime);
            return ThrowOnErrorOrReturnResults(runtime);
        }

        #endregion

        #region internal API
        // these function calls use directly a ProviderRuntime and don't return objects, but write it to the
        // ProviderRuntime's result buffer

        internal void Clear(string[] path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object ClearItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Copy(string[] path, string destinationPath, bool recurse, CopyContainers copyContainers, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object CopyItemDynamicParameters(string[] path, string destination, bool recurse, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal bool Exists(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Get(string[] path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object GetItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Invoke(string[] path, bool literalPath, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object InvokeItemDynamicParameters(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal bool IsContainer(string path, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
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
            // TODO: support globbing (e.g. * in filename)
            Path normalizedPath;
            foreach (var path in paths)
            {
                var provider = GetContainerProviderByPath(path, name, out normalizedPath);
                provider.NewItem(normalizedPath, type, content, runtime);
            }
        }

        internal object NewItemDynamicParameters(string path, string type, object content, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Remove(string[] path, bool recurse, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object RemoveItemDynamicParameters(string path, bool recurse, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Rename(string path, string newName, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object RenameItemDynamicParameters(string path, string newName, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal void Set(string[] path, object value, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        internal object SetItemDynamicParameters(string path, object value, ProviderRuntime runtime)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private helpers

        private ContainerCmdletProvider GetContainerProviderByPath(string path, string name, out Path normalizedPath)
        {
            // TODO: don't use the Path class, use the provider stuff, like the container's MakePath
            PSDriveInfo drive;
            var provider = _cmdlet.State.SessionStateGlobal.GetProviderByPath(path, out drive) as ContainerCmdletProvider;
            if (provider == null)
            {
                throw new PSInvalidOperationException(String.Format("The provider for path '{0}' is not a ContainerProvider", path));
            }
            normalizedPath = new Path(path);
            if (!String.IsNullOrEmpty(name))
            {
                normalizedPath = normalizedPath.Combine(name);
            }
            normalizedPath = normalizedPath.NormalizeSlashes();
            return provider;
        }

        private Collection<PSObject> ThrowOnErrorOrReturnResults(ProviderRuntime runtime)
        {
            runtime.ThrowFirstErrorOrContinue();
            return runtime.RetreiveAllProviderData();
        }

        #endregion
    }
}

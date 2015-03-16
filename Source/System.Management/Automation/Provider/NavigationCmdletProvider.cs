// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation.Provider
{
    public abstract class NavigationCmdletProvider : ContainerCmdletProvider
    {
        protected NavigationCmdletProvider()
        {
        }

        protected virtual string GetChildName(string path) { throw new NotImplementedException(); }
        protected virtual string GetParentPath(string path, string root) { throw new NotImplementedException(); }
        protected virtual bool IsItemContainer(string path) { throw new NotImplementedException(); }
        protected virtual string MakePath(string parent, string child) { throw new NotImplementedException(); }
        protected virtual void MoveItem(string path, string destination) { throw new NotImplementedException(); }
        protected virtual object MoveItemDynamicParameters(string path, string destination) { throw new NotImplementedException(); }
        protected virtual string NormalizeRelativePath(string path, string basePath) { throw new NotImplementedException(); }

        // internals
        internal string GetChildName(string path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return GetChildName(path);
        }

        internal string GetParentPath(string path, string root, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return GetParentPath(path, root);
        }

        internal bool IsItemContainer(string path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return IsItemContainer(path);
        }

        internal string MakePath(string parent, string child, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return MakePath(parent, child);
        }

        internal void MoveItem(string path, string destination, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            MoveItem(path, destination);
        }

        //internal object MoveItemDynamicParameters(string path, string destination, ProviderRuntime providerRuntime);

        internal string NormalizeRelativePath(string path, string basePath, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return NormalizeRelativePath(path, basePath);
        }

    }
}

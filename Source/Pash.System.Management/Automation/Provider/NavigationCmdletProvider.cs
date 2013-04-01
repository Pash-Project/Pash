﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation.Provider
{
    public abstract class NavigationCmdletProvider : ContainerCmdletProvider
    {
        private ProviderRuntime _providerRuntime;

        protected NavigationCmdletProvider()
        {
        }

        protected virtual Path GetChildName(Path path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            return path.GetChildNameOrSelfIfNoChild();
        }

        internal string GetChildName(Path path, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return GetChildName(path);
        }

        protected virtual Path GetParentPath(Path path, Path root)
        {
            if ((root == null) && (PSDriveInfo != null))
            {
                root = PSDriveInfo.Root;
            }


            return path.GetParentPath(root);
        }

        internal string GetParentPath(Path path, string root, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return GetParentPath(path, root);
        }

        protected virtual bool IsItemContainer(Path path)
        {
            throw new NotImplementedException();
        }

        internal bool IsItemContainer(Path path, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return IsItemContainer(path);
        }

        protected virtual string MakePath(Path parent, Path child)
        {
            return parent.Combine(child);
        }

        protected virtual void MoveItem(Path path, Path destination) { throw new NotImplementedException(); }
        protected virtual object MoveItemDynamicParameters(Path path, string destination) { throw new NotImplementedException(); }
        protected virtual string NormalizeRelativePath(Path path, string basePath) { throw new NotImplementedException(); }

        // internals
        //internal string GetChildName(Path path, System.Management.Automation.CmdletProviderContext context);
        //internal string GetParentPath(Path path, string root, System.Management.Automation.CmdletProviderContext context);
        //internal bool IsItemContainer(Path path, System.Management.Automation.CmdletProviderContext context);
        //internal string MakePath(string parent, string child, System.Management.Automation.CmdletProviderContext context);
        //internal void MoveItem(Path path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal object MoveItemDynamicParameters(Path path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal string NormalizeRelativePath(Path path, string basePath, System.Management.Automation.CmdletProviderContext context);

        internal static Path NormalizePath(Path path)
        {
            return PathIntrinsics.NormalizePath(path);
        }
    }
}

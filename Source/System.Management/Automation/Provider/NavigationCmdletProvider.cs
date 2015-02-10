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

        protected virtual string GetChildName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            return new Path(path).GetChildNameOrSelfIfNoChild();
        }

        internal string GetChildName(string path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return GetChildName(path);
        }

        protected virtual string GetParentPath(string path, string root)
        {
            if ((root == null) && (PSDriveInfo != null))
            {
                root = PSDriveInfo.Root;
            }


            return new Path(path).GetParentPath(root);
        }

        internal string GetParentPath(string path, string root, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return GetParentPath(path, root);
        }

        protected virtual bool IsItemContainer(string path)
        {
            throw new NotImplementedException();
        }

        internal bool IsItemContainer(string path, ProviderRuntime providerRuntime)
        {
            ProviderRuntime = providerRuntime;
            return IsItemContainer(path);
        }

        protected virtual string MakePath(string parent, string child)
        {
            return new Path(parent).Combine(child);
        }

        protected virtual void MoveItem(string path, string destination) { throw new NotImplementedException(); }
        protected virtual object MoveItemDynamicParameters(string path, string destination) { throw new NotImplementedException(); }
        protected virtual string NormalizeRelativePath(string path, string basePath) { throw new NotImplementedException(); }

        // internals
        //internal string GetChildName(Path path, System.Management.Automation.CmdletProviderContext context);
        //internal string GetParentPath(Path path, string root, System.Management.Automation.CmdletProviderContext context);
        //internal bool IsItemContainer(Path path, System.Management.Automation.CmdletProviderContext context);
        //internal string MakePath(string parent, string child, System.Management.Automation.CmdletProviderContext context);
        //internal void MoveItem(Path path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal object MoveItemDynamicParameters(Path path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal string NormalizeRelativePath(Path path, string basePath, System.Management.Automation.CmdletProviderContext context);

        internal static string NormalizePath(string path)
        {
            return PathIntrinsics.NormalizePath(path);
        }
    }
}

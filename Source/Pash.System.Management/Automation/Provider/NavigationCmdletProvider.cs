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

        protected virtual string GetChildName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException("Path can't be null");
            }

            path = PathIntrinsics.NormalizePath(path);
            path = path.TrimEnd(PathIntrinsics.CorrectSlash);

            int iLastSlash = path.LastIndexOf('\\');
            if (iLastSlash == -1)
            {
                return path;
            }

            return path.Substring(iLastSlash + 1);
        }

        internal string GetChildName(string path, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return GetChildName(path);
        }

        protected virtual string GetParentPath(string path, string root)
        {
            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("Path can't be empty");

            if ((root == null) && (PSDriveInfo != null))
            {
                root = PSDriveInfo.Root;
            }

            path = PathIntrinsics.NormalizePath(path);
            path = path.TrimEnd(PathIntrinsics.CorrectSlash);

            if (root != null)
            {
                if (string.Equals(path, root, StringComparison.CurrentCultureIgnoreCase))
                {
                    return string.Empty;
                }
            }

            int iLastSlash = path.LastIndexOf(PathIntrinsics.CorrectSlash);

            if (iLastSlash > 0)
                return path.Substring(0, iLastSlash);

            if (iLastSlash == 1)
                return PathIntrinsics.CorrectSlash.ToString();

            return string.Empty;
        }

        internal string GetParentPath(string path, string root, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return GetParentPath(path, root);
        }

        protected virtual bool IsItemContainer(string path)
        {
            throw new NotImplementedException();
        }

        internal bool IsItemContainer(string path, ProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
            return IsItemContainer(path);
        }

        protected virtual string MakePath(string parent, string child)
        {
            if ((parent == null) && (child == null))
            {
                throw new NullReferenceException("Can't call MakePath with null values.");
            }
            if (string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
            {
                return NormalizePath(child);
            }

            parent = NormalizePath(parent);
            if (!string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
            {
                if (parent.EndsWith("\\"))
                {
                    return parent;
                }
                else
                {
                    return parent + '\\';
                }
            }

            child = NormalizePath(child);
            StringBuilder builder = new StringBuilder(parent);

            if (!parent.EndsWith("\\"))
                builder.Append("\\");

            // Make sure we do not add two \
            if (child.StartsWith("\\"))
            {
                builder.Append(child, 1, child.Length - 1);
            }
            else
            {
                builder.Append(child);
            }

            return builder.ToString();
        }

        protected virtual void MoveItem(string path, string destination) { throw new NotImplementedException(); }
        protected virtual object MoveItemDynamicParameters(string path, string destination) { throw new NotImplementedException(); }
        protected virtual string NormalizeRelativePath(string path, string basePath) { throw new NotImplementedException(); }

        // internals
        //internal string GetChildName(string path, System.Management.Automation.CmdletProviderContext context);
        //internal string GetParentPath(string path, string root, System.Management.Automation.CmdletProviderContext context);
        //internal bool IsItemContainer(string path, System.Management.Automation.CmdletProviderContext context);
        //internal string MakePath(string parent, string child, System.Management.Automation.CmdletProviderContext context);
        //internal void MoveItem(string path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal object MoveItemDynamicParameters(string path, string destination, System.Management.Automation.CmdletProviderContext context);
        //internal string NormalizeRelativePath(string path, string basePath, System.Management.Automation.CmdletProviderContext context);

        internal static string NormalizePath(string path)
        {
            return PathIntrinsics.NormalizePath(path);
        }
    }
}

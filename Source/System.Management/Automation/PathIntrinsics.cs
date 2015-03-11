// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;

namespace System.Management.Automation
{
    public sealed class PathIntrinsics
    {
        private SessionStateGlobal _sessionStateGlobal;
        private SessionState _sessionState;

        private PathGlobber _globber;
        private PathGlobber Globber
        {
            get
            {
                if (_globber == null)
                {
                    _globber = new PathGlobber(_sessionState);
                }
                return _globber;
            }
        }

        public PathInfo CurrentFileSystemLocation
        {
            get
            {
                var fsProvider = _sessionState.Provider.GetOne(FileSystemProvider.ProviderName);
                if (fsProvider == null)
                {
                    return null;
                }
                var curDrive = fsProvider.CurrentDrive;
                return new PathInfo(curDrive, curDrive.CurrentLocation, _sessionState);
            }
        }

        public PathInfo CurrentLocation
        {
            get
            {
                return _sessionStateGlobal.CurrentLocation;
            }
        }

        internal PathIntrinsics(SessionState sessionState)
        {
            _sessionState = sessionState;
            _sessionStateGlobal = sessionState.SessionStateGlobal;
        }

        public string Combine(string parent, string child)
        {
            var runtime = new ProviderRuntime(_sessionState);
            return Combine(parent, child, runtime);
        }

        public PathInfo CurrentProviderLocation(string providerName)
        {
            return _sessionStateGlobal.CurrentProviderLocation(providerName);
        }

        public Collection<string> GetResolvedProviderPathFromProviderPath(string path, string providerId)
        {
            throw new NotImplementedException();
        }

        public Collection<string> GetResolvedProviderPathFromPSPath(string path, out ProviderInfo provider)
        {
            throw new NotImplementedException();
        }

        public Collection<PathInfo> GetResolvedPSPathFromPSPath(string path)
        {
            var runtime = new ProviderRuntime(_sessionState);
            var res = GetResolvedPSPathFromPSPath(new [] { path }, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return res;
        }

        public string GetUnresolvedProviderPathFromPSPath(string path)
        {
            throw new NotImplementedException();
        }

        public string GetUnresolvedProviderPathFromPSPath(string path, out ProviderInfo provider, out PSDriveInfo drive)
        {
            throw new NotImplementedException();
        }

        public bool IsProviderQualified(string path)
        {
            throw new NotImplementedException();
        }

        public bool IsPSAbsolute(string path, out string driveName)
        {
            throw new NotImplementedException();
        }

        public bool IsValid(string path)
        {
            var runtime = new ProviderRuntime(_sessionState);
            return IsValid(path, runtime);
        }

        public PathInfoStack LocationStack(string stackName)
        {
            return _sessionStateGlobal.LocationStack(stackName);
        }

        public string NormalizeRelativePath(string path, string basePath)
        {
            var runtime = new ProviderRuntime(_sessionState);
            var res = NormalizeRelativePath(path, basePath, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return res;
        }

        public string ParseChildName(string path)
        {
            var runtime = new ProviderRuntime(_sessionState);
            var res = ParseChildName(path, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return res;
        }

        public string ParseParent(string path, string root)
        {
            throw new NotImplementedException();
        }

        public PathInfo PopLocation(string stackName)
        {
            return _sessionStateGlobal.PopLocation(stackName);
        }

        public void PushCurrentLocation(string stackName)
        {
            _sessionStateGlobal.PushCurrentLocation(stackName);
        }

        public PathInfoStack SetDefaultLocationStack(string stackName)
        {
            return _sessionStateGlobal.SetDefaultLocationStack(stackName);
        }

        public PathInfo SetLocation(string path)
        {
            var runtime = new ProviderRuntime(_sessionState);
            var res = SetLocation(path, runtime);
            runtime.ThrowFirstErrorOrContinue();
            return res;
        }

        // internals
        internal bool IsValid(string path, ProviderRuntime runtime)
        {
            ProviderInfo providerInfo;
            path = new PathGlobber(_sessionState).GetProviderSpecificPath(path, runtime, out providerInfo);
            var provider = _sessionStateGlobal.Provider.GetInstance(providerInfo);
            var itemProvider = CmdletProvider.As<ItemCmdletProvider>(provider);
            return itemProvider.IsValidPath(path, runtime);
        }

        internal string Combine(string parent, string child, ProviderRuntime runtime)
        {
            ProviderInfo providerInfo;
            parent = new PathGlobber(_sessionState).GetProviderSpecificPath(parent, runtime, out providerInfo);
            var provider = _sessionStateGlobal.Provider.GetInstance(providerInfo);
            return Combine(provider, parent, child, runtime);
        }

        internal string Combine(CmdletProvider provider, string parent, string child, ProviderRuntime runtime)
        {
            CmdletProvider.VerifyType<ContainerCmdletProvider>(provider); // throws if it's not
            var navigationPorivder = provider as NavigationCmdletProvider;
            // for a container provider, this is always just the child string (yep, this is PS behavior)
            if (navigationPorivder == null)
            {
                return child;
            }

            // otherwise use the NavigationCmdletProvider's MakePath method
            return navigationPorivder.MakePath(parent, child, runtime);
        }
        //internal Collection<string> GetResolvedProviderPathFromProviderPath(string path, string providerId, ProviderRuntime runtime);
        //internal Collection<string> GetResolvedProviderPathFromPSPath(string path, ProviderRuntime runtime);

        internal Collection<PathInfo> GetResolvedPSPathFromPSPath(string[] paths, ProviderRuntime runtime)
        {
            var resolved = new Collection<PathInfo>();
            foreach (var path in paths)
            {
                CmdletProvider p;
                // by using always a fresh copy of the runtime, we make sure that different paths don't affect each other
                var runtimeCopy = new ProviderRuntime(runtime);
                var globbed = Globber.GetGlobbedProviderPaths(path, runtimeCopy, out p);
                foreach (var curPath in globbed)
                {
                    resolved.Add(new PathInfo(runtimeCopy.PSDriveInfo, curPath, _sessionState));
                }
            }
            return resolved;
        }

        //internal string GetUnresolvedProviderPathFromPSPath(string path, ProviderRuntime runtime, out ProviderInfo provider, out PSDriveInfo drive);
        //internal bool IsCurrentLocationOrAncestor(string path, ProviderRuntime runtime);
       
        internal string NormalizeRelativePath(string path, string basePath, ProviderRuntime runtime)
        {
            // path shouldn't contain wildcards. They are *not* resolved
            ProviderInfo info;
            path = Globber.GetProviderSpecificPath(path, runtime, out info);
            var provider = _sessionState.Provider.GetInstance(info);
            CmdletProvider.VerifyType<ContainerCmdletProvider>(provider);
            var navProvider = provider as NavigationCmdletProvider;

            if (navProvider == null)
            {
                return path;
            }
            return navProvider.NormalizeRelativePath(path, basePath, runtime);
        }

        internal string ParseChildName(string path, ProviderRuntime runtime)
        {
            ProviderInfo info;
            path = Globber.GetProviderSpecificPath(path, runtime, out info);
            var provider = _sessionState.Provider.GetInstance(info);
            return ParseChildName(provider, path, runtime);
        }

        internal string ParseChildName(CmdletProvider provider, string path, ProviderRuntime runtime)
        {
            var navProvider = provider as NavigationCmdletProvider;
            if (navProvider == null)
            {
                return path;
            }
            return navProvider.GetChildName(path, runtime);
        }

        //internal string ParseParent(string path, string root, ProviderRuntime runtime);

        internal PathInfo SetLocation(string path, ProviderRuntime runtime)
        {
            if (path == null)
            {
                throw new PSArgumentException("Path is null", "SetLocationPathNull", ErrorCategory.InvalidArgument);
            }

            ProviderInfo pinfo;
            path = Globber.GetProviderSpecificPath(path, runtime, out pinfo);
            var provider = _sessionState.Provider.GetInstance(pinfo);
            var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
            var itemIntrinsics = new ItemCmdletProviderIntrinsics(_sessionState);

            if (!itemIntrinsics.Exists(path, runtime) ||
                !itemIntrinsics.IsContainer(containerProvider, path, runtime))
            {
                throw new PSArgumentException("The path does not exist or is not a container",
                    "SetLocationInvalidPath", ErrorCategory.InvalidArgument);
            }

            if (provider is FileSystemProvider)
            {
                // TODO: really? I think PS doesn't do this
                System.Environment.CurrentDirectory = path;
            }

            var curDrive = runtime.PSDriveInfo;
            curDrive.CurrentLocation = path;
            _sessionStateGlobal.CurrentDrive = curDrive;
            return new PathInfo(curDrive, path, _sessionState);
        }

        #region Path Operations
        public static readonly char CorrectSlash = System.IO.Path.DirectorySeparatorChar;
        public static readonly char WrongSlash = System.IO.Path.AltDirectorySeparatorChar;

        internal static Path NormalizePath(Path path)
        {
            // TODO: should we normilize the path into a different direction on Unix?
            return path.NormalizeSlashes();
        }

        #endregion

        internal static bool IsAbsolutePath(string path, out string driveName)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path can't be null");
            }

            driveName = null;

            if (path.Length == 0)
            {
                return false;
            }

            if (path.StartsWith("."))
            {
                return false;
            }

            int index = path.IndexOf(":", StringComparison.CurrentCulture);

            if (index > 0)
            {
                driveName = path.Substring(0, index);
                return true;
            }

            return false;
        }

        internal static string RemoveDriveName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith(CorrectSlash.ToString()) || path.StartsWith(WrongSlash.ToString()) || path.StartsWith("."))
                return path;

            int index = path.IndexOf(":", StringComparison.CurrentCulture);

            if (index > 0)
            {
                index++;
                return path.Substring(index, path.Length - index);
            }

            return path;
        }

        internal static string GetFullProviderPath(ProviderInfo provider, string path)
        {
            if (provider == null)
            {
                throw new NullReferenceException("Provider can't be null");
            }
            if (path == null)
            {
                throw new NullReferenceException("Path can't be null");
            }

            string fullPath = path;
            bool hasProviderName = false;

            int index = path.IndexOf("::");
            if (index != -1)
            {
                string providerName = path.Substring(0, index);
                if (provider.IsNameMatch(providerName))
                {
                    hasProviderName = true;
                }
            }

            if (!hasProviderName)
            {
                fullPath = string.Format("{0}::{1}", provider.FullName, path);
            }

            return fullPath;
        }
    }
}

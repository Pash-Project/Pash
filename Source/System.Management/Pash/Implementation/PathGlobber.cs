using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Management;

namespace Pash.Implementation
{
    public class PathGlobber
    {
        private const string _homePlaceholder = "~";
        private static readonly Regex _homePathRegex = new Regex(@"^~[/\\]?");

        private SessionState _sessionState;

        public PathGlobber(SessionState _sessionState)
        {
            this._sessionState = _sessionState;
        }

        internal Collection<string> GetGlobbedProviderPaths(string path, ProviderRuntime runtime,
                                                            out CmdletProvider provider)
        {
            var results = new Collection<string>();
            ProviderInfo providerInfo;

            // get internal path, resolve home path and set provider info and drive info (if resolved)
            path = GetProviderSpecificPath(path, runtime, out providerInfo);

            provider = _sessionState.Provider.GetInstance(providerInfo);

            if (!ShouldGlob(path, runtime))
            {
                results.Add(path);
                return results;
            }

            if (providerInfo.Capabilities.HasFlag(ProviderCapabilities.ExpandWildcards))
            {
                var filter = new IncludeExcludeFilter(runtime.Include, runtime.Exclude, runtime.IgnoreFiltersForGlobbing);
                foreach (var expanded in CmdletProvider.As<ItemCmdletProvider>(provider).ExpandPath(path, runtime))
                {
                    if (filter.Accepts(expanded))
                    {
                        results.Add(expanded);
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Default globbing not yet implemented");
                // TODO: use a default implementation for ContainerCmdletProviders. Also support include/exclude flags
            }

            return results;
        }

        internal bool ShouldGlob(string path, ProviderRuntime runtime)
        {
            return WildcardPattern.ContainsWildcardCharacters(path) && !runtime.AvoidGlobbing.IsPresent;
        }

        internal string GetProviderSpecificPath(string path, ProviderRuntime runtime, out ProviderInfo providerInfo)
        {
            // differentiate between drive-qualified, provider-qualified, provider-internal, and provider-direct paths
            // then strip provider prefix, set provider, set drive is possible or get from Drive.Current
            PSDriveInfo drive;
            if (IsProviderQualifiedPath(path))
            {
                path = GetProviderPathFromProviderQualifiedPath(path, out providerInfo);
                drive = providerInfo.CurrentDrive;
            }
            else if (IsDriveQualifiedPath(path))
            {
                path = GetProviderPathFromDriveQualifiedPath(path, out providerInfo, out drive);
            }
            else if (runtime.PSDriveInfo != null)
            {
                drive = runtime.PSDriveInfo;
                providerInfo = drive.Provider;
            }
            else
            {
                drive = _sessionState.Path.CurrentLocation.Drive;
                providerInfo = _sessionState.Path.CurrentLocation.Provider;
            }
            runtime.PSDriveInfo = drive;
            path = ResolveHomePath(path, providerInfo);
            return path;
        }

        string GetProviderPathFromProviderQualifiedPath(string path, out ProviderInfo providerInfo)
        {
            var idx = path.IndexOf("::");
            var providerId = path.Substring(0, idx);
            providerInfo = _sessionState.Provider.GetOne(providerId);
            return path.Substring(idx + 2);
        }

        string GetProviderPathFromDriveQualifiedPath(string path, out ProviderInfo providerInfo, out PSDriveInfo drive)
        {
            var idx = path.IndexOf(":");
            var driveName = path.Substring(0, idx);
            // TODO: validate drive name?
            drive = _sessionState.Drive.Get(driveName);
            providerInfo = drive.Provider;
            path = path.Substring(idx + 1);
            return new Path(path).NormalizeSlashes().TrimStartSlash().ToString();
        }

        private bool IsProviderQualifiedPath(string path)
        {
            var idx = path.IndexOf("::");
            return idx > 0 && idx + 2 < path.Length;
        }

        private bool IsDriveQualifiedPath(string path)
        {
            var idx = path.IndexOf(":");
            // make sure colon isn't followed by another one
            return idx > 0 && (idx + 1 == path.Length || path[idx + 1] != ':');
        }

        private string ResolveHomePath(string path, ProviderInfo providerInfo)
        {
            if (IsHomePath(path))
            {
                // TODO: substring it and use MakePath if supported
                // path = _homePlaceholder.Replace(path, providerInfo.Home, 1);
                throw new NotImplementedException();
            }
            return path;
        }

        private static bool IsHomePath(string path)
        {
            return _homePathRegex.IsMatch(path);
        }
    }
}


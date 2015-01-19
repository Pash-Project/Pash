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
            PSDriveInfo drive;

            // get internal path, resolve home path and set provider info and drive info (if resolved)
            path = GetProviderSpecificPath(path, out providerInfo, out drive);
            runtime.PSDriveInfo = drive;

            provider = _sessionState.Provider.GetInstance(providerInfo);

            if (!WildcardPattern.ContainsWildcardCharacters(path) || runtime.AvoidWildcardExpansion.IsPresent)
            {
                results.Add(path);
                return results;
            }

            if (providerInfo.Capabilities.HasFlag(ProviderCapabilities.ExpandWildcards))
            {
                foreach (var expanded in CmdletProvider.As<ItemCmdletProvider>(provider).ExpandPath(path, runtime))
                {
                    results.Add(expanded);
                }
            }
            else
            {
                throw new NotImplementedException("Default globbing not yet implemented");
                // TODO: use a default implementation for ContainerCmdletProviders
            }

            // TODO: when to apply the filter?

            return results;
        }

        internal string GetProviderSpecificPath(string path, out ProviderInfo providerInfo, out PSDriveInfo drive)
        {
            // differentiate between drive-qualified, provider-qualified, provider-internal, and provider-direct paths
            // then strip provider prefix, set provider, set drive is possible or get from Drive.Current
            drive = null;
            if (IsProviderQualifiedPath(path))
            {
                path = GetProviderPathFromProviderQualifiedPath(path, out providerInfo);
            }
            else if (IsDriveQualifiedPath(path))
            {
                path = GetProviderPathFromDriveQualifiedPath(path, out providerInfo, out drive);
            }
            else
            {
                providerInfo = _sessionState.Path.CurrentLocation.Provider;
            }
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
            throw new NotImplementedException("No support for drive qualified paths, yet");
        }

        private bool IsProviderQualifiedPath(string path)
        {
            var idx = path.IndexOf("::");
            return idx > 0 && idx + 2 < path.Length;
        }

        private bool IsDriveQualifiedPath(string path)
        {
            var idx = path.IndexOf(":");
            return idx > 0 && idx + 1 < path.Length && path[idx + 1] != ':';
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


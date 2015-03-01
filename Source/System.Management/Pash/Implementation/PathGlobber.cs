using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Management;
using System.Collections.Generic;

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
                results = BuiltInGlobbing(provider, path, runtime);
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
                // in case there is no CurrentDrive, set a dummy drive to keep track of the used provider
                drive = providerInfo.CurrentDrive ?? new PSDriveInfo(providerInfo.Name, providerInfo, "", "", null);
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

        Collection<string> BuiltInGlobbing(CmdletProvider provider, string path, ProviderRuntime runtime)
        {
            var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
            var ciIntrinsics = new ChildItemCmdletProviderIntrinsics(_sessionState);
            var pathIntrinsics = new PathIntrinsics(_sessionState);
            var componentStack = new Stack<string>();
            // first we split the path into globbable components and put them on a stack to work with
            while (!String.IsNullOrEmpty(path))
            {
                var child = ciIntrinsics.GetChildName(path, runtime);
                componentStack.Push(child);
                path = path.Substring(0, path.Length - child.Length).AsPath().TrimEndSlash();
            }

            // we create a working list with partially globbed paths. each iteration will take all items from the
            // list and add the newly globbed part
            var workingPaths = new List<string>() { "" };
            while (componentStack.Count > 0)
            {
                var partialPaths = new List<string>(workingPaths);
                workingPaths.Clear();
                var globComp = componentStack.Pop();
                // check if the current stack component has wildcards. If not, simply append it to all partialPaths
                // and add to workingPaths
                if (!WildcardPattern.ContainsWildcardCharacters(globComp))
                {
                    workingPaths.AddRange(from p in partialPaths select pathIntrinsics.Combine(p, globComp));
                    continue;
                }

                // otherwise get all childnames, check wildcard and combine the paths
                var globWC = new WildcardPattern(globComp, WildcardOptions.IgnoreCase);
                foreach (var partPath in partialPaths)
                {
                    // TODO: verify if we should only consider matching containers or all. maybe the filter won't
                    // apply to partial parts and we need to consider all
                    var childNames = ciIntrinsics.GetValidChildNames(containerProvider, partPath,
                                         ReturnContainers.ReturnMatchingContainers, runtime);
                    // TODO: check if Include/Exclude also match partial parts, but i guess only complete ones
                    // add all combined path to the workingPaths for the next stack globbing iteration
                    workingPaths.AddRange(from c in childNames
                                          where globWC.IsMatch(c)
                                          select pathIntrinsics.Combine(partPath, c, runtime));
                }
            }
            // now filter the working paths by include/exlude. last flag is false or we wouldn't be globbing
            var filter = new IncludeExcludeFilter(runtime.Include, runtime.Exclude, false);
            var globbedPaths = from p in workingPaths where filter.Accepts(p) select p;
            return new Collection<string>(globbedPaths.ToList());
        }
    }
}


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
        private static readonly Regex _homePathRegex = new Regex(@"^~[/\\]?");
        private static readonly Regex _commonRootPathRegex = new Regex(@"^[/\\]");

        private PathIntrinsics Path { get; set; }
        private SessionState _sessionState;

        public PathGlobber(SessionState sessionState)
        {
            _sessionState = sessionState;
            Path = new PathIntrinsics(sessionState);
        }

        internal Collection<string> GetGlobbedProviderPaths(string path, ProviderRuntime runtime,
            out CmdletProvider provider)
        {
            return GetGlobbedProviderPaths(path, runtime, true, out provider);
        }

        internal Collection<string> GetGlobbedProviderPaths(string path, ProviderRuntime runtime, bool itemMustExist,
                                                            out CmdletProvider provider)
        {
            var results = new Collection<string>();
            ProviderInfo providerInfo;

            // get internal path, resolve home path and set provider info and drive info (if resolved)
            path = GetProviderSpecificPath(path, runtime, out providerInfo);
            provider = _sessionState.Provider.GetInstance(providerInfo);

            if (!ShouldGlob(path, runtime))
            {
                // Although even ItemCmdletProvider supports ItemExists, PS doesn't seem
                // to throw errors when resolving paths with ItemProviders, only for ContainerProviders or higher
                // this behavior can be seen in the tests
                var containerProvider = provider as ContainerCmdletProvider;
                if (itemMustExist && containerProvider != null && !containerProvider.ItemExists(path, runtime))
                {
                    var msg = String.Format("An item with path {0} doesn't exist", path);
                    runtime.WriteError(new ItemNotFoundException(msg).ErrorRecord);
                    return results;
                }
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
            string resolvedPath = null;
            if (IsProviderQualifiedPath(path))
            {
                resolvedPath = GetProviderPathFromProviderQualifiedPath(path, out providerInfo);
                // in case there is no CurrentDrive, set a dummy drive to keep track of the used provider
                drive = providerInfo.CurrentDrive ?? providerInfo.DummyDrive;
            }
            else if (IsDriveQualifiedPath(path))
            {
                resolvedPath = GetProviderPathFromDriveQualifiedPath(path, runtime, out providerInfo, out drive);
            }
            // otherwise we first need to know about the provider/drive in use to properly resolve the path
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
            // TODO: check for provider internal path beginning with \\ or //
            //       make sure to set the drive to a dummy drive then

            runtime.PSDriveInfo = drive;

            // if we had no success, yet, we deal with some kind of provider specific (maybe relative) path
            if (resolvedPath == null)
            {
                resolvedPath = ResolveHomePath(path, runtime, providerInfo);
                resolvedPath = ResolveRelativePath(resolvedPath, runtime, providerInfo);
            }

            return resolvedPath;
        }

        internal string GetDriveQualifiedPath(string providerPath, PSDriveInfo drive)
        {
            // expects a provider specific path
            // NOTE: When a drive is hidden, it means that we usually don't want to see the
            // drive qualifier. This is for example the case for the default file system drive
            // on non-Windows systems
            if (drive == null || String.IsNullOrEmpty(drive.Name) || drive.Hidden)
            {
                return providerPath;
            }
            var path = providerPath;
            var sep = PathIntrinsics.CorrectSlash;
            if (path.StartsWith(drive.Root))
            {
                path = path.Substring(drive.Root.Length).TrimStart(sep, PathIntrinsics.WrongSlash);
            }
            return drive.Name + ":" + sep + path;
        }

        internal string GetProviderQualifiedPath(string providerPath, ProviderInfo provider)
        {
            // expects a provider specific path
            return provider.Name + "::" + providerPath;
        }

        string GetProviderPathFromProviderQualifiedPath(string path, out ProviderInfo providerInfo)
        {
            var idx = path.IndexOf("::");
            var providerId = path.Substring(0, idx);
            providerInfo = _sessionState.Provider.GetOne(providerId);
            return path.Substring(idx + 2);
        }

        string GetProviderPathFromDriveQualifiedPath(string path, ProviderRuntime runtime, out ProviderInfo providerInfo, out PSDriveInfo drive)
        {
            var idx = path.IndexOf(":");
            var driveName = path.Substring(0, idx);
            // TODO: validate drive name?
            drive = _sessionState.Drive.Get(driveName);
            providerInfo = drive.Provider;
            path = path.Substring(idx + 1).TrimStart(PathIntrinsics.CorrectSlash, PathIntrinsics.WrongSlash);
            return Path.Combine(providerInfo.CreateInstance(), drive.Root, path, runtime);
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

        private string ResolveHomePath(string path, ProviderRuntime runtime, ProviderInfo providerInfo)
        {
            if (!IsHomePath(path) || providerInfo.Home == null)
            {
                return path;
            }
            var provider = _sessionState.Provider.GetInstance(providerInfo);
            var restPath = path.Substring(1);
            var homePath = providerInfo.Home;
            // if the path is only something like '~/', PS will return only the Home, without trailing separator
            // But if it's '~/foo/', the trailing separator stays
            if (restPath.Length == 0 || restPath.Equals("/") || restPath.Equals("\\"))
            {
                return homePath;
            }
            return Path.Combine(provider, homePath, restPath, runtime);
        }


        private string ResolveRelativePath(string path, ProviderRuntime runtime, ProviderInfo providerInfo)
        {
            var provider = _sessionState.Provider.GetInstance(providerInfo) as ContainerCmdletProvider;
            // TODO: I think PS default relative path resolving can only work with
            // paths that have slash/backslash as spearator. Verify this.
            // skip resolving if the path is absolute or we don't have a navigation provider
            var curRoot = runtime.PSDriveInfo.Root;
            if (provider == null || (curRoot.Length > 0 && path.StartsWith(curRoot)))
            {
                return path;
            }

            if (IsCommonRootPath(path))
            {
                return Path.Combine(provider, curRoot, path, runtime);
            }

            var curPath = runtime.PSDriveInfo.CurrentLocation;

            // we must stay in the same drive. to avoid to resolve to directories outside the drive, we first need to
            // get the path relative to the current drive's root
            // Example: the current drive's root is "/foo", the current location is "/foo/bar". If we resolve "../.."
            //          we need to end up with "/foo", not "/", because "/" is outside the current drive

            // I hope this is just fine... at least PS doesn't call the provider NormalizeRelativePath method
            // for it. If we are in the current location, the path should also start with the drive's root, or
            // we are in some inconsistent state
            if (curPath.StartsWith(curRoot))
            {
                curPath = curPath.Substring(curRoot.Length);
            }

            var relStack = PathToStack(provider, path, runtime);
            while (relStack.Count > 0)
            {
                var comp = relStack.Pop();
                if (comp.Equals("."))
                {
                    continue;
                }
                else if (comp.Equals(".."))
                {
                    curPath = Path.ParseParent(provider, curPath, "", runtime);
                    continue;
                }
                // otherwise it's a child component to append
                curPath = Path.Combine(provider, curPath, comp, runtime);
            }
            // don't forget to append the root again
            return Path.Combine(provider, curRoot, curPath, runtime);
        }

        private static bool IsCommonRootPath(string path)
        {
            return _commonRootPathRegex.IsMatch(path);
        }

        private static bool IsHomePath(string path)
        {
            return _homePathRegex.IsMatch(path);
        }

        Collection<string> BuiltInGlobbing(CmdletProvider provider, string path, ProviderRuntime runtime)
        {
            var containerProvider = CmdletProvider.As<ContainerCmdletProvider>(provider);
            var ciIntrinsics = new ChildItemCmdletProviderIntrinsics(_sessionState);
            // first we split the path into globbable components and put them on a stack to work with
            var componentStack = PathToStack(containerProvider, path, runtime);


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
                    workingPaths.AddRange(from p in partialPaths select Path.Combine(containerProvider, p, globComp, runtime));
                    continue;
                }

                // otherwise get all childnames, check wildcard and combine the paths
                var globWC = new WildcardPattern(globComp, WildcardOptions.IgnoreCase);
                foreach (var partPath in partialPaths)
                {
                    if (!containerProvider.ItemExists(partPath, runtime) ||
                        !containerProvider.HasChildItems(partPath, runtime))
                    {
                        // TODO: throw an error if there was no globbing already performed (then the first part of
                        // the path already did not exists as in a pattern like "/home/notExisting/*.txt"
                        continue;
                    }
                    // TODO: verify if we should only consider matching containers or all. maybe the filter won't
                    // apply to partial parts and we need to consider all
                    var childNames = ciIntrinsics.GetValidChildNames(containerProvider, partPath,
                                         ReturnContainers.ReturnMatchingContainers, runtime);
                    // TODO: check if Include/Exclude also match partial parts, but i guess only complete ones
                    // add all combined path to the workingPaths for the next stack globbing iteration
                    workingPaths.AddRange(from c in childNames
                                          where globWC.IsMatch(c)
                                          select Path.Combine(containerProvider, partPath, c, runtime));
                }
            }
            // now filter the working paths by include/exlude. last flag is false or we wouldn't be globbing
            var filter = new IncludeExcludeFilter(runtime.Include, runtime.Exclude, false);
            var globbedPaths = from p in workingPaths where filter.Accepts(p) select p;
            return new Collection<string>(globbedPaths.ToList());
        }

        private Stack<string> PathToStack(ContainerCmdletProvider provider, string path, ProviderRuntime runtime)
        {
            var componentStack = new Stack<string>();
            while (!String.IsNullOrEmpty(path))
            {
                var child = Path.ParseChildName(provider, path, runtime);
                componentStack.Push(child);
                var parentPath = Path.ParseParent(provider, path, "", runtime);
                if (parentPath.Equals(path))
                {
                    throw new PSInvalidOperationException("Provider's implementation of GetParentPath is inconsistent",
                        "ParentOfPathIsPath", ErrorCategory.InvalidResult);
                }
                path = parentPath;
            }
            return componentStack;
        }
    }
}


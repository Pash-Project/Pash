// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.PowerShell.Commands;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Reflection;
using Extensions.Enumerable;
using System.Text.RegularExpressions;
using System.Management.Automation.Runspaces;

namespace Pash.Implementation
{

    internal class SessionStateGlobal
    {
        //TODO: This and related stuff should be moved to InitialSessionState.CreateDefault
        private string[] _defaultSnapins = new string[] {
            "Microsoft.PowerShell.PSCorePSSnapIn, System.Management.Automation",
            "Microsoft.PowerShell.PSUtilityPSSnapIn, Microsoft.PowerShell.Commands.Utility",
            "Microsoft.Commands.Management.PSManagementPSSnapIn, Microsoft.PowerShell.Commands.Management",
        };
        private Dictionary<string, PSSnapInInfo> _snapins;

        internal readonly ExecutionContext _globalExecutionContext;
        internal SessionState RootSessionState  { get { return _globalExecutionContext.SessionState; } }
        internal PSDriveInfo _currentDrive;

        internal SessionStateGlobal(ExecutionContext executionContext)
        {
            _globalExecutionContext = executionContext;
            _snapins = new Dictionary<string, PSSnapInInfo>(StringComparer.CurrentCultureIgnoreCase);
        }


        #region PSSnapIn specific stuff
        internal void LoadDefaultPSSnapIns()
        {
            foreach (var defaultSnapin in _defaultSnapins)
            {
                Type snapinType = Type.GetType(defaultSnapin, true);
                PSSnapIn snapin = Activator.CreateInstance(snapinType) as PSSnapIn;
                Assembly snapinAssembly = snapinType.Assembly;
                LoadPSSnapIn(new PSSnapInInfo(snapin, snapinAssembly, true), snapinAssembly);
            }
        }

        internal Collection<PSSnapInInfo> GetPSSnapIns(WildcardPattern wildcard)
        {
            Collection<PSSnapInInfo> matches = new Collection<PSSnapInInfo>();
            foreach (var pair in _snapins)
            {
                if (wildcard.IsMatch(pair.Key))
                {
                    matches.Add(pair.Value);
                }
            }
            return matches;
        }

        internal PSSnapInInfo GetPSSnapIn(string name)
        {
            ValidatePSSnapInName(name);
            if (!_snapins.ContainsKey(name))
            {
                return null;
            }
            return _snapins[name];
        }

        internal PSSnapInInfo RemovePSSnapIn(string name)
        {
            ValidatePSSnapInName(name);
            if (!_snapins.ContainsKey(name))
            {
                throw new PSArgumentException(String.Format("PSSnapIn '{0}' is not loaded!", name));
            }
            var snapinInfo = _snapins[name];
            if (snapinInfo.IsDefault)
            {
                throw new PSSnapInException(String.Format("PSSnapIn '{0}' is a built-in snapin and connot be removed!",
                                                          name));
            }
            //unload providers and associated drives
            RootSessionState.Provider.RemoveProviders(snapinInfo);
            
            //unload cmdlets
            RootSessionState.Cmdlet.RemoveAll(snapinInfo);

            _snapins.Remove(name);
            return snapinInfo;
        }

        /// <summary>
        /// Adds the PSSnapIn by name/path
        /// </summary>
        /// <exception cref="PSArgumentException">If loading the snapin fails (e.g. invalid name/path)</exception>
        /// <param name="name">Either the name of the registered snapin or a path to an assembly</param>
        /// <returns>The newly added PSSnapIn</returns>
        internal PSSnapInInfo AddPSSnapIn(string name)
        {
            // a slash is not part of a valid snapin name. If the name contains a slash (e.g. "./foobar") its likely
            // that the user wants to load an assembly directly
            Assembly assembly = null;
            if (name.Contains(PathIntrinsics.CorrectSlash) || name.Contains(PathIntrinsics.WrongSlash))
            {
                assembly = LoadAssemblyFromFile(name);
            }
            else
            {
                assembly = LoadRegisteredPSSnapInAssembly(name);
            }
            //load snapins from assembly and make sure it's only one snapin class defined in thwere
            var snapins = from Type type in assembly.GetTypes()
                          where type.IsSubclassOf(typeof(PSSnapIn))
                          select type;
            if (snapins.Count() != 1)
            {
                string errorMsg = "The assembly '{0}' contains either no or more than one PSSnapIn class!";
                throw new PSSnapInException(String.Format(errorMsg, assembly.FullName));
            }
            PSSnapIn snapin = (PSSnapIn) Activator.CreateInstance(snapins.First());
            //okay, we got the new snapin. now load it
            PSSnapInInfo snapinInfo = new PSSnapInInfo(snapin, assembly, false);
            LoadPSSnapIn(snapinInfo, assembly);
            return snapinInfo;
        }

        private void LoadPSSnapIn(PSSnapInInfo snapinInfo, Assembly assembly)
        {
            var snapinName = snapinInfo.Name;
            try
            {
                _snapins.Add(snapinName, snapinInfo);
            }
            catch (ArgumentException)
            {
                throw new PSSnapInException(String.Format("The snapin '{0}' is already loaded!", snapinName));
            }
            RootSessionState.Provider.LoadProvidersFromAssembly(assembly, snapinInfo);
            RootSessionState.Cmdlet.LoadCmdletsFromAssembly(assembly, snapinInfo);
        }

        Assembly LoadAssemblyFromFile(string name)
        {
            try
            {
                return Assembly.LoadFrom(name);
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new PSArgumentException("The assembly was not found!", e);
            }
            catch (Exception e)
            {
                throw new PSSnapInException(String.Format("Loading the assembly '{0}' failed!", name), e);
            }
        }

        Assembly LoadRegisteredPSSnapInAssembly(string name)
        {
            ValidatePSSnapInName(name);
            throw new NotImplementedException();
            //TODO: support snapins registered in registry: read registry, get assembly path and load with
            //return LoadAssemblyFromFile(name);
        }

        private void ValidatePSSnapInName(string name)
        {
            if (!Regex.IsMatch(name, "[a-zA-Z0-9._]+"))
            {
                throw new PSArgumentException(String.Format("Invalid PSSnapIn name '{0}'", name));
            }
        }
        #endregion

        #region Current Location Management
        internal PathInfo CurrentLocation
        {
            get
            {
                if (CurrentDrive == null)
                {
                    throw new NullReferenceException("CurrentDrive is null.");
                }
                PSDriveInfo driveInfo = CurrentDrive;
                return new PathInfo(driveInfo, driveInfo.Provider, driveInfo.CurrentLocation, _globalExecutionContext.SessionState);
            }
        }

        public PSDriveInfo CurrentDrive
        {
            get
            {
                return _currentDrive;
            }
            private set
            {
                _currentDrive = value;
            }
        }

        internal void SetCurrentDrive()
        {
            ProviderInfo fileSystemProvider = RootSessionState.Provider.GetOne(FileSystemProvider.ProviderName);
            if (fileSystemProvider == null)
                throw new Exception("FileSystemProvider not found.");

            // Initialize the _currentDrive and set it's currentlocation 
            // to the same as the current environment directory
            foreach (var drive in fileSystemProvider.Drives)
            {
                Path currentDir = System.Environment.CurrentDirectory;
                if (string.Equals(drive.Name, currentDir.GetDrive(), StringComparison.OrdinalIgnoreCase))
                {
                    drive.CurrentLocation = currentDir;
                    _currentDrive = drive;
                    return;
                }
            }

            throw new NotImplementedException("why are we here?");
        }
        #endregion

        #region PathIntrinsics
        //TODO: move this (and implement it) in the appropriate class, not here
        internal string MakePath(string parent, string child)
        {
            throw new NotImplementedException();
        }

        internal PathInfo CurrentProviderLocation(string providerName)
        {
            throw new NotImplementedException();
        }

        internal bool IsValidPath(string path)
        {
            throw new NotImplementedException();
        }

        internal PathInfoStack LocationStack(string stackName)
        {
            throw new NotImplementedException();
        }

        internal string NormalizeRelativePath(string path, string basePath)
        {
            throw new NotImplementedException();
        }

        internal string GetPathChildName(string path)
        {
            throw new NotImplementedException();
        }

        internal string GetParentPath(string path, string root)
        {
            throw new NotImplementedException();
        }

        internal PathInfo PopLocation(string stackName)
        {
            throw new NotImplementedException();
        }

        internal void PushCurrentLocation(string stackName)
        {
            throw new NotImplementedException();
        }

        internal PathInfoStack SetDefaultLocationStack(string stackName)
        {
            throw new NotImplementedException();
        }

        internal PathInfo SetLocation(string path)
        {
            return SetLocation(path, new ProviderRuntime(_globalExecutionContext));
        }
        #endregion

        // TODO: it would be nice if these functions would be in the intrinsics itself, not called from there
        internal Collection<PSObject> GetChildItems(string path, bool recurse)
        {
            ProviderRuntime providerRuntime = new ProviderRuntime(_globalExecutionContext);

            return GetChildItems(path, recurse, providerRuntime);
        }

        internal Collection<PSObject> GetChildItems(string path, bool recurse, ProviderRuntime providerRuntime)
        {
            if (string.IsNullOrEmpty(path))
                path = CurrentLocation.Path;

            PSDriveInfo drive;
            CmdletProvider provider = GetProviderByPath(path, out drive);

            if ((path != null) && (ItemExists(provider, path, providerRuntime)))
            {
                if (IsItemContainer(provider, path, providerRuntime))
                {
                    ContainerCmdletProvider containerProvider = provider as ContainerCmdletProvider;

                    if (containerProvider != null)
                        containerProvider.GetChildItems(path, recurse, providerRuntime);
                }
                else
                {
                    ItemCmdletProvider itemProvider = provider as ItemCmdletProvider;

                    if (itemProvider != null)
                        itemProvider.GetItem(path, providerRuntime);
                }
            }

            return providerRuntime.RetreiveAllProviderData();
        }

        internal CmdletProvider GetProviderByPath(Path path, out PSDriveInfo drive)
        {
            // MUST: implement for "dir"
            if (string.IsNullOrEmpty(path))
                path = CurrentLocation.Path;

            drive = GetDrive(path);

            if (drive == null)
            {
                drive = CurrentLocation.Drive;
            }


            if (drive == null)
                return null;

            return RootSessionState.Provider.GetProviderInstance(drive.Provider.Name);
        }

        private PSDriveInfo GetDrive(Path path)
        {
            string driveName = path.GetDrive();
            if (!string.IsNullOrEmpty(driveName))
            {
                return RootSessionState.Drive.Get(driveName);
            }
            return null;
        }

        private bool ItemExists(CmdletProvider provider, string path, ProviderRuntime providerRuntime)
        {
            ItemCmdletProvider itemProvider = provider as ItemCmdletProvider;

            if (itemProvider == null)
                return false;

            return itemProvider.ItemExists(path, providerRuntime);
        }

        private bool IsItemContainer(CmdletProvider provider, string path, ProviderRuntime providerRuntime)
        {
            NavigationCmdletProvider navigationProvider = provider as NavigationCmdletProvider;

            if (navigationProvider == null)
                return false;

            return navigationProvider.IsItemContainer(path, providerRuntime);
        }

        internal Collection<string> GetChildNames(string path, ReturnContainers returnContainers, bool recurse)
        {
            // MUST: fix
            throw new NotImplementedException();
        }

        internal Collection<string> GetChildNames(string path, ReturnContainers returnContainers, bool recurse, ProviderRuntime providerRuntime)
        {
            // MUST: fix
            throw new NotImplementedException();
        }

        internal PathInfo SetLocation(Path path, ProviderRuntime providerRuntime)
        {
            // TODO: deal with paths starting with ".\"

            if (path == null)
            {
                throw new NullReferenceException("Path can't be null");
            }

            PSDriveInfo nextDrive = CurrentDrive;

            // use the same provider-specific logic as resolve-path would use here
            path = path.NormalizeSlashes().ResolveTilde();

            string driveName = null;
            if (path.TryGetDriveName(out driveName))
            {
                try
                {
                    nextDrive = _globalExecutionContext.SessionState.Drive.Get(driveName);
                }
                catch (MethodInvocationException) //didn't find a drive (maybe it's "\" on Windows)
                {
                    nextDrive = CurrentDrive;
                }
            }

            Path newLocation = PathNavigation.CalculateFullPath(nextDrive.CurrentLocation, path);

            var provider = RootSessionState.Provider.GetInstance(nextDrive.Provider);
            if (!(provider is ItemCmdletProvider))
            {
                throw new PSInvalidOperationException("Cannot set location for this type of provider.");
            }
            var itemProvider = (ItemCmdletProvider)provider;

            if (!itemProvider.ItemExists(newLocation, providerRuntime))
            {
                throw new PSInvalidOperationException(string.Format("Cannot find path '{0}' because it does not exist.", newLocation));
            }

            if (provider is FileSystemProvider)
            {
                System.Environment.CurrentDirectory = newLocation;
            }

            nextDrive.CurrentLocation = newLocation;

            CurrentDrive = nextDrive;
            return CurrentLocation;
        }
    }
}

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
        //TODO: I don't think that this information should stay here. Maybe RunspaceConfiguration or InitialSessionState
        //would be better place, once they are implemented
        private string[] _defaultSnapins = new string[] {
            "Microsoft.PowerShell.PSCorePSSnapIn, System.Management.Automation",
            "Microsoft.PowerShell.PSUtilityPSSnapIn, Microsoft.PowerShell.Commands.Utility",
            "Microsoft.Commands.Management.PSManagementPSSnapIn, Microsoft.PowerShell.Commands.Management",
        };
        //TODO: I really don't know why we should support lists of providers with the same name. This isn't reasonable!
        private Dictionary<string, List<ProviderInfo>> _providers;
        private Dictionary<string, List<CmdletProvider>> _providerInstances;
        private Dictionary<ProviderInfo, PSDriveInfo> _providersCurrentDrive;
        private Dictionary<string, Stack<PathInfo>> _workingLocationStack;
        private Dictionary<string, PSSnapInInfo> _snapins;

        private ExecutionContext _executionContext;
        internal PSDriveInfo _currentDrive;

        internal SessionStateGlobal(ExecutionContext executionContext)
        {
            _executionContext = executionContext;
            _providers = new Dictionary<string, List<ProviderInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _providerInstances = new Dictionary<string, List<CmdletProvider>>(StringComparer.CurrentCultureIgnoreCase);
            _providersCurrentDrive = new Dictionary<ProviderInfo, PSDriveInfo>();
            _workingLocationStack = new Dictionary<string, Stack<PathInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _snapins = new Dictionary<string, PSSnapInInfo>(StringComparer.CurrentCultureIgnoreCase);
        }

        #region CmdletProviderManagementIntrinsics
        //TODO: this should go into CmdletProviderManagementIntrinsics
        internal IEnumerable<ProviderInfo> Providers
        {
            get
            {
                Collection<ProviderInfo> collection = new Collection<ProviderInfo>();
                foreach (List<ProviderInfo> list in _providers.Values)
                {
                    foreach (ProviderInfo info in list)
                    {
                        collection.Add(info);
                    }
                }
                return collection;
            }
        }

        internal ProviderInfo GetProviderByName(string name)
        {
            Collection<ProviderInfo> providers = GetProvidersByName(name);

            if (providers.Count > 0)
                return providers[0];

            return null;
        }

        internal Collection<ProviderInfo> GetProvidersByName(string name)
        {
            if (_providers.ContainsKey(name))
                return new Collection<ProviderInfo>(_providers[name]);

            return new Collection<ProviderInfo>();
        }

        private Collection<ProviderInfo> GetProvidersByPSSnapIn(PSSnapInInfo snapinInfo)
        {
            Collection<ProviderInfo> snapinProviders = new Collection<ProviderInfo>();
            foreach (var pair in _providers)
            {
                foreach (var provider in pair.Value)
                {
                    if (provider.PSSnapIn.Equals(snapinInfo))
                    {
                        snapinProviders.Add(provider);
                    }
                }
            }
            return snapinProviders;
        }
        #endregion

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
            foreach (var provider in GetProvidersByPSSnapIn(snapinInfo))
            {
                try
                {
                    RemoveProvider(provider.Name);
                }
                catch (ProviderNotFoundException)
                {
                    //TODO: it would be great to have the possibilities to write warnings here. It'd be nice notifying
                    //the user about this strange effect, but isn't worth aborting the whole action
                }
            }
            
            //unload cmdlets
            CommandManager.RemoveCmdlets(snapinInfo);

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
            LoadProvidersFromAssembly(assembly, snapinInfo);
            CommandManager.LoadCmdletsFromAssembly(assembly, snapinInfo);
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

        #region Provider's Initialization
        //TODO: Move this provider stuff to ProviderIntrinsics and access it through SessionState.Provider
        internal CommandManager CommandManager
        {
            get
            {
                //TODO: somehow this doesn't look like a good idea, we should change that and get this class
                //independent from the CommandManager, so the CM should check out the SessionStateGlobal, not the other
                //way around
                return ((LocalRunspace)_executionContext.CurrentRunspace).CommandManager;
            }
        }

        private void RemoveProvider(string name)
        {
            if (!_providers.ContainsKey(name))
            {
                throw new ProviderNotFoundException(name, SessionStateCategory.CmdletProvider, "RemoveProviderNotFound",
                                                    null);
            }
            //remove all drives. TODO: I think _providers[name].Drive
            foreach (var drive in _executionContext.SessionState.Drive.GetAllForProvider(name))
            {
                //remove from all scopes, sub-scopes might created a new drive using this provider
                _executionContext.SessionState.Drive.RemoveAtAllScopes(drive);
            }

            //now also stop and remove all instances
            if (_providerInstances.ContainsKey(name))
            {
                var instances = _providerInstances[name];
                foreach (var inst in instances)
                {
                    //TODO: it should be possible to notify the provider that it's removed. That's also the intention
                    //of the provider's Stop() method. However, the specification says that Stop() is protected. So
                    //we need another, internal method, that can be called. I called it DoStop(), but this should
                    //certainly be changed. Like ghaving an internal Stop() method with a useful argument
                    inst.DoStop();
                }
            }
             _providers.Remove(name);
        }

        private CmdletProvider AddProvider(ProviderInfo providerInfo)
        {
            CmdletProvider provider = providerInfo.CreateInstance();

            provider.Start(providerInfo, new ProviderRuntime(_executionContext));
            provider.SetProviderInfo(providerInfo);

            // Cache the Provider's Info
            if (!_providers.ContainsKey(providerInfo.Name))
            {
                _providers.Add(providerInfo.Name, new List<ProviderInfo>());
            }
            _providers[providerInfo.Name].Add(providerInfo);

            return provider;
        }


        private void InitializeProvider(CmdletProvider providerInstance, ProviderInfo provider)
        {
            List<PSDriveInfo> drives = new List<PSDriveInfo>();
            DriveCmdletProvider driveProvider = providerInstance as DriveCmdletProvider;

            if (driveProvider != null)
            {
                Collection<PSDriveInfo> collection = driveProvider.DoInitializeDefaultDrives();
                if ((collection != null) && (collection.Count > 0))
                {
                    drives.AddRange(collection);
                    _providersCurrentDrive[provider] = collection[0];
                }
            }

            if (drives.Count > 0)
            {
                foreach (PSDriveInfo driveInfo in drives)
                {
                    if (driveInfo != null)
                    {
                        // TODO: need to set driveInfo.Root
                        // TODO:this should be automatically called when using DriveIntrinsics.New! #providerSupport
                        driveProvider.DoNewDrive(driveInfo);

                        try
                        {
                            //always to global scope
                            _executionContext.SessionState.Drive.New(driveInfo,
                                SessionStateScope<PSDriveInfo>.ScopeSpecifiers.Global.ToString());
                        }
                        catch
                        {
                            // TODO: What should we do if the drive name is not unique?
                            // => I guess overwrite the old one and write a warning (however this works from here)
                        }
                    }
                }
            }
        }

        private void LoadProvidersFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo)
        {
            // first get name and type of all providers in this assembly
            var providers = from Type type in assembly.GetTypes()
                where !type.IsSubclassOf(typeof(Cmdlet))
                    where type.IsSubclassOf(typeof(CmdletProvider))
                    from CmdletProviderAttribute providerAttr in
                    type.GetCustomAttributes(typeof(CmdletProviderAttribute), true)
                    select new KeyValuePair<string, Type>(providerAttr.ProviderName, type);
            // then initialize all providers
            foreach (var curPair in providers)
            {
                ProviderInfo providerInfo = new ProviderInfo(_executionContext.SessionState, curPair.Value,
                                                             curPair.Key, string.Empty, snapinInfo);
                CmdletProvider provider = AddProvider(providerInfo);
                InitializeProvider(provider, providerInfo);

                // Cache the provider's instance
                if (!_providerInstances.ContainsKey(providerInfo.Name))
                {
                    _providerInstances.Add(providerInfo.Name, new List<CmdletProvider>());
                }
                List<CmdletProvider> instanceList = _providerInstances[providerInfo.Name];
                instanceList.Add(provider);
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
                return new PathInfo(driveInfo, driveInfo.Provider, driveInfo.CurrentLocation, _executionContext.SessionState);
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
            ProviderInfo fileSystemProvider = GetProviderByName(FileSystemProvider.ProviderName);
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
            return SetLocation(path, new ProviderRuntime(_executionContext));
        }
        #endregion

        internal CmdletProvider GetProviderInstance(string name)
        {
            if (_providerInstances.ContainsKey(name))
            {
                List<CmdletProvider> instanceList = _providerInstances[name];

                if (instanceList.Count > 0)
                {
                    // Take the first one
                    return instanceList[0] as CmdletProvider;
                }
            }

            return null;
        }

        // TODO: it would be nice if these functions would be in the intrinsics itself, not called from there
        internal Collection<PSObject> GetChildItems(string path, bool recurse)
        {
            ProviderRuntime providerRuntime = new ProviderRuntime(_executionContext);

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

            return GetProviderInstance(drive.Provider.Name);
        }

        private PSDriveInfo GetDrive(Path path)
        {
            string driveName = path.GetDrive();
            if (!string.IsNullOrEmpty(driveName))
            {
                return _executionContext.SessionState.Drive.Get(driveName);
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

            if (path == "~")
            {
                // Older Mono versions (sadly the one that's currently still
                // available) have a bug where GetFolderPath returns an empty
                // string for most SpecialFolder values, but only on
                // non-Windows.
                // See: https://bugzilla.xamarin.com/show_bug.cgi?id=2873

                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // HACK: Use $Env:HOME until Mono 2.10 dies out.
                if (path == "")
                    path = Environment.GetEnvironmentVariable("HOME");
            }

            PSDriveInfo nextDrive = CurrentDrive;

            path = path.NormalizeSlashes();

            string driveName = null;
            if (path.TryGetDriveName(out driveName))
            {
                try
                {
                    nextDrive = _executionContext.SessionState.Drive.Get(driveName);
                }
                catch (MethodInvocationException) //didn't find a drive (maybe it's "\" on Windows)
                {
                    nextDrive = CurrentDrive;
                }
            }

            Path newLocation = PathNavigation.CalculateFullPath(nextDrive.CurrentLocation, path);

            // I'm not a fan of this block of code.
            // The goal here is to throw an exception if trying to "CD" into an invalid location
            //
            // Not sure why the providerInstances are returned as a collection. Feels like given a 
            // path we should have one provider we're talking to.
            if (_providerInstances.ContainsKey(nextDrive.Provider.Name))
            {
                bool pathExists = false;
                IEnumerable<ItemCmdletProvider> cmdletProviders = _providerInstances[nextDrive.Provider.Name].Where(x => x is ItemCmdletProvider).Cast<ItemCmdletProvider>();
                ItemCmdletProvider currentProvider = null;
                foreach (var provider in cmdletProviders)
                {
                    if (provider.ItemExists(newLocation, providerRuntime))
                    {
                        pathExists = true;
                        currentProvider = provider;
                        break;
                    }
                }

                if (!pathExists)
                {
                    throw new Exception(string.Format("Cannot find path '{0}' because it does not exist.", newLocation));
                }
                else
                {
                    if (currentProvider is FileSystemProvider)
                    {
                        System.Environment.CurrentDirectory = newLocation;
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Unsure how to set location with provider:" + nextDrive.Provider.Name);
            }

            nextDrive.CurrentLocation = newLocation;

            CurrentDrive = nextDrive;
            _providersCurrentDrive[CurrentDrive.Provider] = CurrentDrive;
            return CurrentLocation;
        }
    }
}

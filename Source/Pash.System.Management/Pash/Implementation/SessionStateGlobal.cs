using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections.Specialized;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;

namespace Pash.Implementation
{
    internal class SessionStateGlobal
    {
        private Dictionary<string, List<ProviderInfo>> _providers;
        private Dictionary<string, List<CmdletProvider>> _providerInstances;
        private Dictionary<ProviderInfo, PSDriveInfo> _providersCurrentDrive;
        private Dictionary<string, PSDriveInfo> _drives;
        private Dictionary<string, Stack<PathInfo>> _workingLocationStack;
        private Dictionary<string, AliasInfo> _aliases;
        private Dictionary<string, CommandInfo> _functions;
        private HybridDictionary _variables;
        private ExecutionContext _executionContext;
        internal PSDriveInfo _currentDrive;

        private SessionStateGlobal()
        {
            _providers = new Dictionary<string, List<ProviderInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _providerInstances = new Dictionary<string, List<CmdletProvider>>(StringComparer.CurrentCultureIgnoreCase);
            _providersCurrentDrive = new Dictionary<ProviderInfo, PSDriveInfo>();
            _drives = new Dictionary<string, PSDriveInfo>(StringComparer.CurrentCultureIgnoreCase);
            _workingLocationStack = new Dictionary<string, Stack<PathInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _aliases = new Dictionary<string, AliasInfo>(StringComparer.CurrentCultureIgnoreCase);
            _functions = new Dictionary<string, CommandInfo>(StringComparer.CurrentCultureIgnoreCase);
            _variables = new HybridDictionary(true);
        }

        internal SessionStateGlobal(ExecutionContext context)
            : this()
        {
            _executionContext = context;
            if (context.SessionState == null)
                return;
            if (context.SessionState.SessionStateGlobal == null)
                return;
            _providers = context.SessionState.SessionStateGlobal._providers;
            _providersCurrentDrive = context.SessionState.SessionStateGlobal._providersCurrentDrive;
            _workingLocationStack = context.SessionState.SessionStateGlobal._workingLocationStack;
            _drives = context.SessionState.SessionStateGlobal._drives;
            _aliases = context.SessionState.SessionStateGlobal._aliases;
            _functions = context.SessionState.SessionStateGlobal._functions;
            _variables = context.SessionState.SessionStateGlobal._variables;
        }

        #region CmdletProviderManagementIntrinsics
        internal IEnumerable<ProviderInfo> Providers
        {
            get
            {
                Collection<ProviderInfo> collection = new Collection<ProviderInfo>();
                foreach (List<ProviderInfo> list in _providers.Values)
                {
                    foreach(ProviderInfo info in list)
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
        #endregion

        #region Provider's Initialization
        /// <summary>
        /// Loads providers from the PSSnapins on the Pash startup
        /// </summary>
        internal void LoadProviders()
        {
            // TODO: do it via RunspaceConfiguration
            /*foreach (ProviderConfigurationEntry entry in _executionContext.RunspaceConfiguration.Providers)
            {
                //AddProvider
            }*/

            CommandManager cmdMgr = ((LocalRunspace)_executionContext.CurrentRunspace).CommandManager;
            foreach (var providerTypePair in cmdMgr._providers)
            {
                ProviderInfo providerInfo = new ProviderInfo(new SessionState(this), providerTypePair.providerType, providerTypePair.providerAttr.ProviderName, string.Empty, providerTypePair.snapinInfo);
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

                        driveProvider.DoNewDrive(driveInfo);

                        try
                        {
                            _drives.Add(driveInfo.Name, driveInfo);
                        }
                        catch
                        {
                            // TODO: What should we do if the drive name is not unique?
                        }
                    }
                }
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
                return new PathInfo(driveInfo, driveInfo.Provider, driveInfo.CurrentLocation, new SessionState(this));
            }
        }

        public PSDriveInfo CurrentDrive
        {
            get
            {
                return _currentDrive;
            }
        }

        internal void SetCurrentDrive()
        {
            ProviderInfo fileSystemProvider = GetProviderByName(FileSystemProvider.ProviderName);
            if (fileSystemProvider == null)
                //throw new Exception("Can't find the FileSystemProvider");
                return;

            Collection<PSDriveInfo> drives = fileSystemProvider.Drives;
            if ((drives != null) && (drives.Count > 0))
            {
                // Select the first one as a default
                _currentDrive = drives[0];

                // Traverse all the available drives and choose the "Fixed" one
                foreach (PSDriveInfo drive in drives)
                {
                    if (! drive.RemovableDrive)
                    {
                        _currentDrive = drive;
                        break;
                    }
                }
            }

        }
        #endregion

        #region PSVariableIntrinsics
        internal PSVariable GetVariable(string name)
        {
            // TODO: deal with scopes

            if (_variables.Contains(name))
                return _variables[name] as PSVariable;

            return null;
        }

        internal object GetVariableValue(string name)
        {
            throw new NotImplementedException();
        }

        internal object GetVariableValue(string name, object defaultValue)
        {
            throw new NotImplementedException();
        }

        internal void RemoveVariable(PSVariable variable)
        {
            RemoveVariable(variable.Name);
        }

        internal void RemoveVariable(string name)
        {
            if (_variables.Contains(name))
                _variables.Remove(name);
        }

        internal object SetVariable(PSVariable variable)
        {
            // TODO: deal with scopes

            RemoveVariable(variable);
        
            _variables.Add(variable.Name, variable);

            return variable;
        }

        internal object SetVariable(string name, object value)
        {
            PSVariable variable = GetVariable(name);

            if (variable != null)
                RemoveVariable(variable);
            
            variable = new PSVariable(name, value);

            _variables.Add(variable.Name, variable);

            return variable;
        }

        #endregion

        #region PathIntrinsics
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

        #region DriveManagementIntrinsics
        internal PSDriveInfo GetDrive(string driveName, string scope)
        {
            if (! _drives.ContainsKey(driveName))
                return null;

            return _drives[driveName];
        }

        /// <summary>
        /// Returns a list of physical file system drives, including all the mounted removable drives
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        internal Collection<PSDriveInfo> GetDrives(string scope)
        {
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            foreach (PSDriveInfo info in _drives.Values)
            {
                // TODO: make sure not to include the removed drives or to
                // TODO: make sure to mount all the removable drives
                //PSDriveInfo psDriveInfo = MountFileSystemDrive(driveInfo);
                //if (psDriveInfo != null)
                //{
                //    drivesList[psDriveInfo.Name] = psDriveInfo;
                //}

                collection.Add(info);
            }

            return collection;
        }

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

        //private PSDriveInfo MountFileSystemDrive(DriveInfo driveInfo)
        //{
        //    PSDriveInfo newDrive = null;

        //    ProviderInfo providerInfo = GetProviderByName(FileSystemProvider.ProviderName);
        //    // If no FileSystem provider - do nothing
        //    if (providerInfo == null)
        //    {
        //        return newDrive;
        //    }
        //    try
        //    {
        //        DriveCmdletProvider driveProviderInstance = GetDriveProviderInstance(FileSystemProvider.ProviderName);
        //        if (driveProviderInstance != null)
        //        {
        //            PSDriveInfo drive = new PSDriveInfo(driveInfo.Name.Substring(0, 1), driveProviderInstance.ProviderInfo, driveInfo.RootDirectory.FullName, driveInfo.VolumeLabel, null);
        //            drive.RemovableDrive = driveInfo.DriveType != DriveType.Fixed;

        //            newDrive = drive;
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //    }
        //    return newDrive;
        //}

        internal Collection<PSDriveInfo> GetDrivesForProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                return GetDrives(null);
            }
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            foreach (PSDriveInfo driveInfo in GetDrives(null))
            {
                if ((driveInfo != null) && driveInfo.Provider.IsNameMatch(providerName))
                {
                    collection.Add(driveInfo);
                }
            }
            return collection;
        }

        internal PSDriveInfo AddNewDrive(PSDriveInfo drive, string scope)
        {
            throw new NotImplementedException();
        }

        internal void RemoveDrive(string driveName, bool force, string scope)
        {
            throw new NotImplementedException();
        }
        #endregion

        internal IDictionary GetVariables()
        {
            Dictionary<string, PSVariable> dictionary = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in _variables )
            {
                if (dictionary.ContainsKey((string)entry.Key))
                {
                    continue;
                }
                PSVariable variable = (PSVariable)entry.Value;
                if (! variable.IsPrivate)
                {
                    dictionary.Add((string)entry.Key, (PSVariable)entry.Value);
                }
            }
            return dictionary;

        }

        internal IDictionary GetFunctions()
        {
            Dictionary<string, CommandInfo> dictionary = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (CommandInfo info in _functions.Values)
            {
                if (!dictionary.ContainsKey(info.Name))
                {
                    dictionary.Add(info.Name, info);
                }
            }
            return dictionary;
        }

        internal void RemoveFunction(string name)
        {
            throw new NotImplementedException();
        }


        internal void NewVariable(PSVariable variable, bool force)
        {
            throw new NotImplementedException();
        }

        internal Collection<PSObject> GetChildItems(string path, bool recurse)
        {
            ProviderRuntime providerRuntime = new ProviderRuntime(_executionContext);

            return GetChildItems(path, recurse, providerRuntime);
        }

        internal Collection<PSObject> GetChildItems(string path, bool recurse, ProviderRuntime providerRuntime)
        {
            if (string.IsNullOrEmpty(path))
                path = CurrentLocation.Path;

            CmdletProvider provider = GetProviderByPath(path);

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

        private CmdletProvider GetProviderByPath(string path)
        {
            // MUST: implement for "dir"
            if (string.IsNullOrEmpty(path))
                path = CurrentLocation.Path;

            string driveName = PathIntrinsics.GetDriveFromPath(path);

            PSDriveInfo drive = GetDrive(driveName, null);

            if (drive == null)
                return null;

            return GetProviderInstance(drive.Provider.Name);
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

        internal PathInfo SetLocation(string path, ProviderRuntime providerRuntime)
        {
            // TODO: deal with paths starting with ".\"

            if (path == null)
            {
                throw new NullReferenceException("Path can't be null");
            }

            path = PathIntrinsics.NormalizePath(path);

            ProviderInfo provider = null;
            string driveName = null;

            string str = path;
            string providerId = null;
            PSDriveInfo currentDrive = CurrentDrive;

            // If path doesn't start with a drive name
            if (path.StartsWith(PathIntrinsics.CorrectSlash.ToString()))
            {
                provider = CurrentLocation.Provider;
            }
            else if (PathIntrinsics.IsAbsolutePath(path, out driveName))
            {
                _currentDrive = GetDrive(driveName, null);

                path = PathIntrinsics.NormalizePath(PathIntrinsics.RemoveDriveName(path));
            }

            _currentDrive.CurrentLocation = path;

            _providersCurrentDrive[CurrentDrive.Provider] = CurrentDrive;
            
            SetVariable("PWD", CurrentLocation);
            return CurrentLocation;


            PSDriveInfo drive = CurrentDrive;

            SetLocation(path);
        }
    }
}
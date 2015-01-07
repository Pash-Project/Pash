// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;
using Pash.Implementation;
using System.Reflection;

namespace System.Management.Automation
{
    public sealed class CmdletProviderManagementIntrinsics
    {
        private SessionStateGlobal _sessionState;
        private Dictionary<string, List<ProviderInfo>> _providers;
        private Dictionary<string, List<CmdletProvider>> _providerInstances;
        //private Dictionary<ProviderInfo, PSDriveInfo> _providersCurrentDrive;

        // Powershell seems to support multiple providers with the same name. So let's do it too

        internal CmdletProviderManagementIntrinsics(SessionStateGlobal sessionState)
        {
            _sessionState = sessionState;
            _providers = new Dictionary<string, List<ProviderInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _providerInstances = new Dictionary<string, List<CmdletProvider>>(StringComparer.CurrentCultureIgnoreCase);
            //_providersCurrentDrive = new Dictionary<ProviderInfo, PSDriveInfo>();
        }

        public Collection<ProviderInfo> Get(string name)
        {
            return GetProvidersByName(name);
        }

        public IEnumerable<ProviderInfo> GetAll()
        {
            return Providers;
        }

        public ProviderInfo GetOne(string name)
        {
            return GetProviderByName(name);
        }

        //TODO: this should go into CmdletProviderManagementIntrinsics
        internal IEnumerable<ProviderInfo> Providers
        {
            get
            {
                List<ProviderInfo> list = new List<ProviderInfo>();
                foreach (List<ProviderInfo> sublist in _providers.Values)
                {
                    list.AddRange(sublist);
                }
                return list;
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

        internal Collection<ProviderInfo> GetProviders(PSSnapInInfo snapinInfo)
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

        private void RemoveProvider(string name)
        {
            if (!_providers.ContainsKey(name))
            {
                throw new ProviderNotFoundException(name, SessionStateCategory.CmdletProvider, "RemoveProviderNotFound",
                    null);
            }
            //remove all drives. TODO: I think _providers[name].Drive
            foreach (var drive in _sessionState.RootSessionState.Drive.GetAllForProvider(name))
            {
                //remove from all scopes, sub-scopes might created a new drive using this provider
                _sessionState.RootSessionState.Drive.RemoveAtAllScopes(drive);
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

            provider.Start(providerInfo, new ProviderRuntime(_sessionState._globalExecutionContext));
            provider.SetProviderInfo(providerInfo);

            // Cache the Provider's Info
            if (!_providers.ContainsKey(providerInfo.Name))
            {
                _providers.Add(providerInfo.Name, new List<ProviderInfo>());
            }
            _providers[providerInfo.Name].Add(providerInfo);

            return provider;
        }

        internal CmdletProvider GetInstance(ProviderInfo info)
        {
            // TODO: do it better
            return _providerInstances[info.Name].First();
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
                    //_providersCurrentDrive[provider] = collection[0];
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
                            _sessionState.RootSessionState.Drive.New(driveInfo,
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

        internal void RemoveProviders(PSSnapInInfo snapinInfo)
        {
            foreach (var provider in GetProviders(snapinInfo))
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
        }

        internal void LoadProvidersFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo)
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
                ProviderInfo providerInfo = new ProviderInfo(_sessionState.RootSessionState, curPair.Value,
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


    }
}

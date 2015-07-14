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
        // Powershell seems to support multiple providers with the same name. So let's do it too
        // TODO: move the actual data to SessionStateGlobal
        private Dictionary<string, List<ProviderInfo>> _providers;
        private Dictionary<ProviderInfo, CmdletProvider> _providerInstances;

        internal CmdletProviderManagementIntrinsics(SessionStateGlobal sessionState)
        {
            _sessionState = sessionState;
            _providers = new Dictionary<string, List<ProviderInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _providerInstances = new Dictionary<ProviderInfo, CmdletProvider>();
        }

        #region public API

        public Collection<ProviderInfo> Get(string name)
        {
            if (_providers.ContainsKey(name))
                return new Collection<ProviderInfo>(_providers[name]);

            return new Collection<ProviderInfo>();
        }

        public IEnumerable<ProviderInfo> GetAll()
        {
            List<ProviderInfo> list = new List<ProviderInfo>();
            foreach (List<ProviderInfo> sublist in _providers.Values)
            {
                list.AddRange(sublist);
            }
            return list;
        }

        public ProviderInfo GetOne(string name)
        {
            return Get(name).FirstOrDefault();
        }

        #endregion

        #region internal functionality

        internal Collection<ProviderInfo> Get(PSSnapInInfo snapinInfo)
        {
            var providers = from p in GetAll()
                            where p.PSSnapIn != null && p.PSSnapIn.Equals(snapinInfo)
                            select p;
            return new Collection<ProviderInfo>(providers.ToList());
        }

        internal Collection<ProviderInfo> Get(PSModuleInfo moduleInfo)
        {
            var providers = from p in GetAll()
                            where p.Module != null && p.Module.Equals(moduleInfo)
                            select p;
            return new Collection<ProviderInfo>(providers.ToList());
        }

        internal CmdletProvider GetInstance(string name)
        {
            if (!_providers.ContainsKey(name))
            {
                return null;
            }
            List<ProviderInfo> providerList = _providers[name];

            if (providerList.Count > 0)
            {
                // take first one or null
                return _providerInstances[providerList[0]];
            }

            return null;
        }

        internal CmdletProvider GetInstance(ProviderInfo info)
        {
            return _providerInstances[info];
        }

        internal void Remove(PSModuleInfo moduleInfo, ExecutionContext executionContext)
        {
            foreach (var provider in Get(moduleInfo))
            {
                try
                {
                    Remove(provider, executionContext);
                }
                catch (ProviderNotFoundException)
                {
                    //TODO: it would be great to have the possibilities to write warnings here. It'd be nice notifying
                    //the user about this strange effect, but isn't worth aborting the whole action
                }
            }
        }

        internal void Remove(PSSnapInInfo snapinInfo, ExecutionContext executionContext)
        {
            foreach (var provider in Get(snapinInfo))
            {
                try
                {
                    Remove(provider, executionContext);
                }
                catch (ProviderNotFoundException)
                {
                    //TODO: it would be great to have the possibilities to write warnings here. It'd be nice notifying
                    //the user about this strange effect, but isn't worth aborting the whole action
                }
            }
        }

        internal void Load(Assembly assembly, ExecutionContext executionContext, PSModuleInfo moduleInfo)
        {
            Load(assembly, executionContext, null, moduleInfo);
        }

        internal void Load(Assembly assembly, ExecutionContext executionContext, PSSnapInInfo snapinInfo)
        {
            Load(assembly, executionContext, snapinInfo, null);
        }

        // private method avoids that accidentally both snapin and module get specified
        private void Load(Assembly assembly, ExecutionContext executionContext, PSSnapInInfo snapinInfo, PSModuleInfo moduleInfo)
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
                                    curPair.Key, string.Empty, string.Empty, string.Empty, snapinInfo, moduleInfo);
                Add(providerInfo, executionContext);
            }
        }

        internal CmdletProvider Add(ProviderInfo providerInfo, ExecutionContext executionContext)
        {
            CmdletProvider provider = providerInfo.CreateInstance();

            var runtime = new ProviderRuntime(executionContext.SessionState);
            providerInfo = provider.Start(providerInfo, runtime);
            provider.SetProviderInfo(providerInfo);

            // Cache the Provider's Info and instance
            if (!_providers.ContainsKey(providerInfo.Name))
            {
                _providers.Add(providerInfo.Name, new List<ProviderInfo>());
            }
            _providers[providerInfo.Name].Add(providerInfo);
            _providerInstances[providerInfo] = provider;

            // provider is added, default drives can be added
            AddDefaultDrives(provider, runtime);

            return provider;
        }

        #endregion

        #region private helper functions

        private void Remove(ProviderInfo info, ExecutionContext executionContext)
        {
            var runtime = new ProviderRuntime(executionContext.SessionState);
            //remove all drives. TODO: I think _providers[name].Drive
            foreach (var drive in _sessionState.RootSessionState.Drive.GetAllForProvider(info.FullName))
            {
                //remove from all scopes, sub-scopes might created a new drive using this provider
                _sessionState.RootSessionState.Drive.RemoveAtAllScopes(drive, runtime);
            }

            //now also stop and remove
            var inst = _providerInstances[info];
            inst.Stop(runtime);
            _providerInstances.Remove(info);

            var list = _providers[info.Name];
            if (list == null) // shouldn't happen, but who knows
            {
                return;
            }
            list.Remove(info);
        }

        private void AddDefaultDrives(CmdletProvider providerInstance, ProviderRuntime runtime)
        {
            DriveCmdletProvider driveProvider = providerInstance as DriveCmdletProvider;
            if (driveProvider == null)
            {
                return;
            }

            var drives = driveProvider.InitializeDefaultDrives(runtime);
            if (drives == null)
            {
                throw new PSInvalidOperationException("The default drive collection for this null!");
            }

            if (drives.Count == 0)
            {
                drives = providerInstance.GetDriveFromProviderInfo();
            }

            foreach (PSDriveInfo driveInfo in drives)
            {
                if (driveInfo == null)
                {
                    continue;
                }
                try
                {
                    // always to global scope
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

        #endregion

    }
}

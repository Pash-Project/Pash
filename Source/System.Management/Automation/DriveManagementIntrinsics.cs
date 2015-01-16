// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    public sealed class DriveManagementIntrinsics
    {
        private const string driveDoesntExistFormat = @"No such drive. A drive with the name ""{0}"" doesn't exist.";
        private const string driveAlreadyExistsFormat = @"A drive with the name ""{0}"" already exists.";

        private SessionStateScope<PSDriveInfo> _scope;

        internal DriveManagementIntrinsics(SessionStateScope<PSDriveInfo> driveScope)
        {
            _scope = driveScope;
        }

        public PSDriveInfo Current
        {
            get
            {
                return _scope.SessionState.SessionStateGlobal.CurrentDrive;
            }
        }

        internal bool TryGet(string driveName, out PSDriveInfo info)
        {
            info = _scope.Get(driveName, false);
            return info != null;
        }

        public PSDriveInfo Get(string driveName)
        {
            var drive = _scope.Get(driveName, false);
            if (drive == null)
            {
                //PS 2.0 throws an exception here instead of returning null
                throw new MethodInvocationException(String.Format(driveDoesntExistFormat, driveName));
            }
            return drive;
        }

        public Collection<PSDriveInfo> GetAll()
        {
            return new Collection<PSDriveInfo>(_scope.GetAll().Values.ToList());
        }

        public Collection<PSDriveInfo> GetAllAtScope(string scope)
        {
            if (String.IsNullOrEmpty(scope))
            {
                 //this behavior corresponds to PS 2.0
                return GetAll();
            }
            return new Collection<PSDriveInfo>(_scope.GetAllAtScope(scope).Values.ToList());
        }

        public Collection<PSDriveInfo> GetAllForProvider(string providerName)
        {
            if (String.IsNullOrEmpty(providerName))
            {
                //this behavior corresponds to PS 2.0
                return GetAll();
            }
            /* "fun" fact: this method is different from getting the provider and checking its DriveProperty!
            * ProviderInfo.Drives seems to only provide drives that are available in the 0 scope
            * However, this functions needs to provide also the drives of child scopes that are associated with a
            * provider.
            * That's why sessionScope.SessionStateGlobal.GetProviderByName(providerName).Drives doesn't work here
            * At least that's the behavior of PS 2.0
            */
            var allDrives = GetAll();
            Collection<PSDriveInfo> providerDrives = new Collection<PSDriveInfo>();
            foreach (var drive in allDrives)
            {
                if (drive.Provider.IsNameMatch(providerName))
                {
                    providerDrives.Add(drive);
                }
            }
            //no drives means we don't have a proper provider!
            if (providerDrives.Count < 1)
            {
                throw new MethodInvocationException(
                    String.Format(@"No such provider. A provider with the name ""{0}"" doesn't exist.", providerName)
                );
            }
            return providerDrives;
        }

        public PSDriveInfo GetAtScope(string driveName, string scope)
        {
            var info =_scope.GetAtScope(driveName, scope);
            if (info == null)
            {
                throw new MethodInvocationException(String.Format(driveDoesntExistFormat, driveName));
            }
            return info;
        }

        public PSDriveInfo New(PSDriveInfo drive, string scope)
        {
            // we take the Runspace.DefaultRunspace.ExecutionContext because on every #ExecutionContextChange
            // this variable is set to the current
            var runtime = new ProviderRuntime(Runspace.DefaultRunspace.ExecutionContext);
            return New(drive, scope, runtime);
        }

        internal PSDriveInfo New(PSDriveInfo drive, string scope, ProviderRuntime providerRuntime)
        {
            // make sure the provider can intitalize this drive properly
            drive = GetProvider(drive).NewDrive(drive, providerRuntime);
            return NewSkipInit(drive, scope);
        }

        internal PSDriveInfo NewSkipInit(PSDriveInfo drive, string scope)
        {
            /*
             * "Fun" Fact: Although "private" is a valid scope specifier, it does not really make the drive
             * private, i.e. it does not restricts child scopes froma accessing or removing it.
             * "Private" seems to be only effective for variables, functions and aliases, but not for drives.
             * Who knows why.
             */
            _scope.SetAtScope(drive, scope, false);
            return drive;
        }

        public void Remove(string driveName, bool force, string scope)
        {
            // we take the Runspace.DefaultRunspace.ExecutionContext because on every #ExecutionContextChange
            // this variable is set to the current
            var runtime = new ProviderRuntime(Runspace.DefaultRunspace.ExecutionContext);
            runtime.Force = new SwitchParameter(force);
            Remove(driveName, scope, runtime);
        }

        internal void Remove(string driveName, string scope, ProviderRuntime runtime)
        {
            /* TODO: force is used to remove the drive "although it's in use by the provider"
             * So, we need to find out when a drive is in use and should throw an exception on removal without
             * the "force" parameter being true
             */
            var drive = _scope.GetAtScope(driveName, scope);
            if (drive == null)
            {
                throw new DriveNotFoundException(driveName, String.Empty, null);
            }

            // make sure the provider can clean up this drive properly
            GetProvider(drive).RemoveDrive(drive, runtime);
            _scope.RemoveAtScope(driveName, scope);
        }

        internal void RemoveAtAllScopes(PSDriveInfo drive, ProviderRuntime runtime)
        {
            // don't forget to give the provider the chance to clean up first
            GetProvider(drive).RemoveDrive(drive, runtime);
            foreach (var curScope in _scope.HierarchyIterator)
            {
                if (curScope.HasLocal(drive))
                {
                    curScope.RemoveLocal(drive.Name);
                }
            }
        }

        DriveCmdletProvider GetProvider(PSDriveInfo drive)
        {
            var provider = _scope.SessionState.Provider.GetInstance(drive.Provider) as DriveCmdletProvider;
            if (provider == null)
            {
                throw new ArgumentException("No proper DriveCmdletProvider is associated with drive '" + drive.Name + "'.");
            }
            return provider;
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Collections.Generic;
using System.Linq;

namespace System.Management.Automation
{
    public sealed class DriveManagementIntrinsics
    {
        private const string driveDoesntExistFormat = @"No such drive. A drive with the name ""{0}"" doesn't exist.";
        private const string driveAlreadyExistsFormat = @"A drive with the name ""{0}"" already exists.";

        private SessionStateScope sessionScope;

        internal DriveManagementIntrinsics(SessionStateScope scope)
        {
            sessionScope = scope;
        }

        public PSDriveInfo Current
        {
            get
            {
                return sessionScope.SessionStateGlobal.CurrentDrive;
            }
        }

        public PSDriveInfo Get(string driveName)
        {
            //recursively look for the drive
            for (var candidate = sessionScope; candidate != null; candidate = candidate.ParentScope)
            {
                var info = candidate.GetLocalDrive(driveName);
                if (info != null)
                {
                    return info;
                }
            }
            //no such drive found
            throw new MethodInvocationException(String.Format(driveDoesntExistFormat, driveName));
        }

        public Collection<PSDriveInfo> GetAll()
        {
            //first get a copy of the drives in the local scope. NOTE: it also copies the correct StringComparer
            //dictionary first, so ContainsKey is faster than in a collection
            var visibleDrives = new Dictionary<string, PSDriveInfo>(sessionScope.LocalDrives);
            //recursively gather all drives from parent scopes
            for (var cur = sessionScope.ParentScope; cur != null; cur = cur.ParentScope)
            {
                foreach (KeyValuePair<string, PSDriveInfo> pair in cur.LocalDrives)
                {
                    //as ascend the scope hierarchy, we must not overwrite values of child scopes
                    if (!visibleDrives.ContainsKey(pair.Key))
                    {
                        visibleDrives.Add(pair.Key, pair.Value);
                    }
                }
            }
            return new Collection<PSDriveInfo>(visibleDrives.Values.ToList());
        }

        public Collection<PSDriveInfo> GetAllAtScope(string scope)
        {
            if (String.IsNullOrEmpty(scope))
            {
                 //this behavior corresponds to PS 2.0
                return GetAll();
            }
            SessionStateScope affectedScope = sessionScope.GetScope(scope);
            return new Collection<PSDriveInfo>(affectedScope.LocalDrives.Values.ToList());
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
            SessionStateScope affectedScope = sessionScope.GetScope(scope);
            var info = affectedScope.GetLocalDrive(driveName);
            if (info == null)
            {
                throw new MethodInvocationException(String.Format(driveDoesntExistFormat, driveName));
            }
            return info;
        }

        public PSDriveInfo New(PSDriveInfo drive, string scope)
        {
            SessionStateScope affectedScope = sessionScope.GetScope(scope);
            if (!affectedScope.AddLocalDrive(drive))
            {
                throw new MethodInvocationException(String.Format(driveAlreadyExistsFormat, drive.Name));
            }
            return drive;
        }

        public void Remove(string driveName, bool force, string scope)
        {
            SessionStateScope affectedScope = sessionScope.GetScope(scope);
            if (!affectedScope.RemoveLocalDrive(driveName, force))
            {
                throw new MethodInvocationException(String.Format(driveDoesntExistFormat, driveName));
            }
        }
    }
}

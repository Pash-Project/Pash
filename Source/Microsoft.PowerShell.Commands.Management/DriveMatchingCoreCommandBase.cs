// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Linq;

namespace Microsoft.PowerShell.Commands
{
    public class DriveMatchingCoreCommandBase : CoreCommandBase
    {
        public DriveMatchingCoreCommandBase()
        {

        }

        internal List<PSDriveInfo> GetDrives(string[] literalName, string[] name, string[] providerNames, string scope)
        {
            var drives = String.IsNullOrEmpty(scope) ? SessionState.Drive.GetAll() 
                                                     : SessionState.Drive.GetAllAtScope(scope);
            var filtered = FilterDriveByProvider(drives, providerNames);
            filtered = FilterDriveByName(filtered, literalName, name);
            return filtered.ToList();
        }


        internal IEnumerable<PSDriveInfo> FilterDriveByProvider(IEnumerable<PSDriveInfo> drives, string[] providerNames)
        {
            if (providerNames == null)
            {
                return drives;
            }
            return from d in drives where d.Provider.IsAnyNameMatch(providerNames) select d;
        }

        internal IEnumerable<PSDriveInfo> FilterDriveByName(IEnumerable<PSDriveInfo> drives, string[] literalName, string[] name)
        {
            if (literalName != null)
            {
                return from d in drives
                       where literalName.Contains(d.Name, StringComparer.InvariantCultureIgnoreCase)
                       select d;
            }
            if (name == null || name.Length == 0)
            {
                return drives;
            }
            var wildcards = (from n in name select new WildcardPattern(n, WildcardOptions.IgnoreCase)).ToArray();
            return from d in drives
                   where WildcardPattern.IsAnyMatch(wildcards, d.Name)
                   select d;
        }
    }
}

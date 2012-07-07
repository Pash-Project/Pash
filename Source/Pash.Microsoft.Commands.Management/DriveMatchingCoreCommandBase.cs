using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class DriveMatchingCoreCommandBase : CoreCommandBase
    {
        public DriveMatchingCoreCommandBase()
        {

        }

        internal List<PSDriveInfo> GetDrivesByName(string driveName, string[] providerNames)
        {
            List<PSDriveInfo> list = new List<PSDriveInfo>();
            if ((providerNames == null) || (providerNames.Length == 0))
            {
                // TODO: make sure to find the drive names via patterns
                providerNames = new string[] {""};
            }
            foreach (string str in providerNames)
            {
                // TODO: try to get the drive from all the providers SessionState.Provider.Get(providerName)

                foreach (PSDriveInfo info in SessionState.Drive.GetAll())
                {
                    if (string.Equals(info.Name, driveName, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(info);
                    }
                }
            }
            list.Sort();
            return list;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Provider
{
    public abstract class DriveCmdletProvider : CmdletProvider
    {
        protected DriveCmdletProvider()
        {
        }

        protected virtual Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo>();
        }

        protected virtual PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            return drive;
        }

        protected virtual object NewDriveDynamicParameters()
        {
            throw new NotImplementedException();
        }

        protected virtual PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            throw new NotImplementedException();
        }

        // internals
        //internal System.Collections.ObjectModel.Collection<PSDriveInfo> InitializeDefaultDrives(CmdletProviderContext context);
        //internal PSDriveInfo NewDrive(PSDriveInfo drive, CmdletProviderContext context);
        //internal object NewDriveDynamicParameters(CmdletProviderContext context);
        //internal PSDriveInfo RemoveDrive(PSDriveInfo drive, CmdletProviderContext context);

        internal Collection<PSDriveInfo> DoInitializeDefaultDrives()
        {
            return InitializeDefaultDrives();
        }

        internal PSDriveInfo DoNewDrive(PSDriveInfo drive)
        {
            return NewDrive(drive);
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Pash.Implementation;

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
            return null;
        }

        protected virtual PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            return drive; //nothing special to do per default
        }

        internal PSDriveInfo RemoveDrive(PSDriveInfo drive, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return RemoveDrive(drive);
        }

        internal Collection<PSDriveInfo> InitializeDefaultDrives(ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return InitializeDefaultDrives();
        }

        internal PSDriveInfo NewDrive(PSDriveInfo drive, ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return NewDrive(drive);
        }

        internal object NewDriveDynamicParameters(ProviderRuntime runtime)
        {
            ProviderRuntime = runtime;
            return NewDriveDynamicParameters();
        }
    }
}

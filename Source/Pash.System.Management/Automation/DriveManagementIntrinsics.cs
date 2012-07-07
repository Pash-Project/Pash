using System;
using System.Collections.ObjectModel;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class DriveManagementIntrinsics
    {
        private SessionStateGlobal _sessionState;

        internal DriveManagementIntrinsics(SessionStateGlobal sessionState)
        {
            _sessionState = sessionState;
        }

        public PSDriveInfo Current
        {
            get
            {
                return _sessionState.CurrentDrive;
            }
        }

        public PSDriveInfo Get(string driveName)
        {
            return _sessionState.GetDrive(driveName, null);
        }

        public Collection<PSDriveInfo> GetAll()
        {
            return _sessionState.GetDrives(null);
        }

        public Collection<PSDriveInfo> GetAllAtScope(string scope)
        {
            return _sessionState.GetDrives(scope);
        }

        public Collection<PSDriveInfo> GetAllForProvider(string providerName)
        {
            return _sessionState.GetDrivesForProvider(providerName);
        }

        public PSDriveInfo GetAtScope(string driveName, string scope)
        {
            return _sessionState.GetDrive(driveName, scope);
        }

        public PSDriveInfo New(PSDriveInfo drive, string scope)
        {
            return _sessionState.AddNewDrive(drive, scope);
        }

        public void Remove(string driveName, bool force, string scope)
        {
            _sessionState.RemoveDrive(driveName, force, scope);
        }

        // internals
        //internal void New(PSDriveInfo drive, string scope, CmdletProviderContext context);
        //internal object NewDriveDynamicParameters(string providerId, CmdletProviderContext context);
        //internal void Remove(string driveName, bool force, string scope, CmdletProviderContext context);
    }
}

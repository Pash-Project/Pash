// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.CompilerServices;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class PathInfo
    {
        public PSDriveInfo Drive { get; private set; }

        private string _driveQualified;
        private string _providerQualified;
        private string _path;
        public string Path { get; private set; }

        public ProviderInfo Provider { get; private set; }
        public string ProviderPath
        {
            get
            {
                return _providerQualified;
            }
        }

        internal PathInfo(PSDriveInfo drive, string path, SessionState sessionState)
        {
            Drive = drive;
            Provider = drive == null ? null : drive.Provider;
            SetPathTypes(path, sessionState);
        }

        void SetPathTypes(string path, SessionState sessionState)
        {
            ProviderInfo pinfo;
            // use the globber to parse the path and set the different types
            var runtime = new ProviderRuntime(sessionState);
            runtime.PSDriveInfo = Drive;
            var globber = new PathGlobber(sessionState);
            _path = globber.GetProviderSpecificPath(path, runtime, out pinfo);
            // update the Provider and Drive in case it changed
            Provider = pinfo;
            Drive = runtime.PSDriveInfo;

            _providerQualified = globber.GetProviderQualifiedPath(_path, Provider);
            Path = _providerQualified;

            if (Drive != null && !String.IsNullOrEmpty(Drive.Name))
            {
                _driveQualified = globber.GetDriveQualifiedPath(_path, Drive);
                Path = _driveQualified;
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

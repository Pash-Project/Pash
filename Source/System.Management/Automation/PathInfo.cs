// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.CompilerServices;

namespace System.Management.Automation
{
    public sealed class PathInfo
    {
        public PSDriveInfo Drive { get; private set; }

        private Path _path;
        public string Path
        {
            get
            {
                return _path.MakePath(Drive.Name).ToString();
            }
        }

        public ProviderInfo Provider { get; private set; }
        public string ProviderPath
        {
            get
            {
                // TODO: get provider path from SessionState
                throw new NotImplementedException();
            }
        }

        private SessionState _sessionState;

        internal PathInfo(PSDriveInfo drive, ProviderInfo provider, Path path, SessionState sessionState)
        {
            Drive = drive;
            Provider = provider;
            _path = path;
            _sessionState = sessionState;
        }

        public override string ToString()
        {
            return Path.ToString();
        }
    }
}

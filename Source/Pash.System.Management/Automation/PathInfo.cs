// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Runtime.CompilerServices;

namespace System.Management.Automation
{
    public sealed class PathInfo
    {
        public PSDriveInfo Drive { get; private set; }

        private string _path;
        public string Path
        {
            get
            {
                return PathIntrinsics.MakePath(_path, Drive);
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

        internal PathInfo(PSDriveInfo drive, ProviderInfo provider, string path, SessionState sessionState)
        {
            Drive = drive;
            Provider = provider;
            _path = path;
            _sessionState = sessionState;
        }

        public override string ToString()
        {
            return this.Path;
        }
    }
}

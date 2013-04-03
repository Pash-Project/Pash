// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    /// <summary>
    /// Exposes functionality to cmdlets from providers.
    /// </summary>
    public sealed class ContentCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void Clear(string path) { throw new NotImplementedException(); }
        public Collection<IContentReader> GetReader(string path) { throw new NotImplementedException(); }
        public Collection<IContentWriter> GetWriter(string path) { throw new NotImplementedException(); }
    }
}

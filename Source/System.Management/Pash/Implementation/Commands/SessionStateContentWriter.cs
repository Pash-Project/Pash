// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;

namespace System.Management.Pash.Implementation
{
    class SessionStateContentWriter : IContentWriter
    {
        SessionStateProviderBase _provider;
        Path _path;

        internal SessionStateContentWriter(SessionStateProviderBase provider, Path path)
        {
            _provider = provider;
            _path = path;
        }

        public void Close()
        {
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public IList Write(IList content)
        {
            _provider.SetSessionStateItem(_path, content, false);
            return content;
        }

        public void Dispose()
        {
        }
    }
}

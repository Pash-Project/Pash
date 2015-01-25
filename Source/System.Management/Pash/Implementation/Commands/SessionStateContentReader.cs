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
    class SessionStateContentReader : IContentReader
    {
        SessionStateProviderBase _provider;
        Path _path;
        bool readItem;

        internal SessionStateContentReader(SessionStateProviderBase provider, Path path)
        {
            _provider = provider;
            _path = path;
        }

        public void Close()
        {
        }

        public IList Read(long readCount)
        {
            var items = new List<object>();

            if (!readItem)
            {
               readItem = true;
                object item = _provider.GetSessionStateItem(_path);
                items.Add(_provider.GetValueOfItem(item));
            }

            return items;
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Close();
        }
    }
}

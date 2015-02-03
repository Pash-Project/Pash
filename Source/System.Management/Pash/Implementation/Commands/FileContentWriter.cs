// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;
using System.Text;

namespace System.Management.Pash.Implementation
{
    class FileContentWriter : IContentWriter
    {
        string _fileName;
        StreamWriter _writer;

        internal FileContentWriter(string fileName)
        {
            // Default file encoding is ASCII.
            this._fileName = fileName;
            _writer = new StreamWriter(fileName, true, Encoding.ASCII);
        }

        public void Close()
        {
            _writer.Close();
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public IList Write(IList content)
        {
            foreach (object item in content)
            {
                _writer.WriteLine(item);
            }
            return content;
        }

        public void Dispose()
        {
            Close();
        }
    }
}

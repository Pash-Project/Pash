// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;

namespace System.Management.Pash.Implementation
{
    class FileContentReader : IContentReader
    {
        string _fileName;
        List<string> lines;
        int currentLine;

        internal FileContentReader(string fileName)
        {
            this._fileName = fileName;
        }

        public void Close()
        {
        }

        public IList Read(long readCount)
        {
            if (lines == null)
            {
                lines = File.ReadLines(_fileName).ToList();
            }

            int start = currentLine;
            currentLine += (int)readCount;
            return lines.Skip(start).Take((int)readCount).ToList();
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

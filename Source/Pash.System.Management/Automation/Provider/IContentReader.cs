using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace System.Management.Automation.Provider
{
    public interface IContentReader : IDisposable
    {
        void Close();
        IList Read(long readCount);
        void Seek(long offset, SeekOrigin origin);
    }
}

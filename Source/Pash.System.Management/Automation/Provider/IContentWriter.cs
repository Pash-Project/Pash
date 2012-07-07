using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace System.Management.Automation.Provider
{
    public interface IContentWriter : IDisposable
    {
        void Close();
        void Seek(long offset, SeekOrigin origin);
        IList Write(IList content);
    }
}

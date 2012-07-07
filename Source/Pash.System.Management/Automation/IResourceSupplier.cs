using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public interface IResourceSupplier
    {
        string GetResourceString(string baseName, string resourceId);
    }
}

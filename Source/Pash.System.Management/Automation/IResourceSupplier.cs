// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Provider
{
    public interface IContentCmdletProvider
    {
        void ClearContent(string path);
        object ClearContentDynamicParameters(string path);
        IContentReader GetContentReader(string path);
        object GetContentReaderDynamicParameters(string path);
        IContentWriter GetContentWriter(string path);
        object GetContentWriterDynamicParameters(string path);
    }
}

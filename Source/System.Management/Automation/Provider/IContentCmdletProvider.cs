// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Provider
{
    public interface IContentCmdletProvider
    {
        void ClearContent(Path path);
        object ClearContentDynamicParameters(Path path);
        IContentReader GetContentReader(Path path);
        object GetContentReaderDynamicParameters(Path path);
        IContentWriter GetContentWriter(Path path);
        object GetContentWriterDynamicParameters(Path path);
    }
}

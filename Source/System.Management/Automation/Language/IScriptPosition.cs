// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    public interface IScriptPosition
    {
        int ColumnNumber { get; }
        string File { get; }
        string Line { get; }
        int LineNumber { get; }
        int Offset { get; }

        string GetFullScript();
    }
}

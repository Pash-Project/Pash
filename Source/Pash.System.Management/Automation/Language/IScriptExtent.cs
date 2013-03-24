// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    public interface IScriptExtent
    {
        int EndColumnNumber { get; }
        int EndLineNumber { get; }
        int EndOffset { get; }
        IScriptPosition EndScriptPosition { get; }
        string File { get; }
        int StartColumnNumber { get; }
        int StartLineNumber { get; }
        int StartOffset { get; }
        IScriptPosition StartScriptPosition { get; }
        string Text { get; }
    }
}

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

using System;

namespace System.Management.Automation.Language
{
    public enum AstVisitAction
    {
        Continue = 0,
        SkipChildren = 1,
        StopVisit = 2,
    }
}

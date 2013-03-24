// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

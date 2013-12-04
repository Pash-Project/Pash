// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal interface IScriptBlockInfo
    {
        ScopeUsages ScopeUsage { get; }
        ScriptBlock ScriptBlock { get; }
        ReadOnlyCollection<ParameterAst> GetParameters();
    }
}



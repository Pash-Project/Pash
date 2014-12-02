// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections;
using System.Management.Automation.Language;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public class FilterInfo : FunctionInfo
    {
        internal FilterInfo(string name, ScriptBlock filter, IEnumerable<ParameterAst> parameters)
            : base(name, filter, parameters)
        {
        }
    }
}


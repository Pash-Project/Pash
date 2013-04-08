// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace System.Management.Automation
{
    public class FilterInfo : FunctionInfo
    {
        internal FilterInfo(string name, ScriptBlock filter)
            : base(name, filter)
        {
        }
    }
}


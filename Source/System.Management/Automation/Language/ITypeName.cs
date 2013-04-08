// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    public interface ITypeName
    {
        string AssemblyName { get; }
        IScriptExtent Extent { get; }
        string FullName { get; }
        bool IsArray { get; }
        bool IsGeneric { get; }
        string Name { get; }

        Type GetReflectionAttributeType();
        Type GetReflectionType();
    }
}

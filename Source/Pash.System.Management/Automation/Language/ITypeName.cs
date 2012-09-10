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

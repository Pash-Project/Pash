// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation
{
    public class PSTypeName
    {
        public PSTypeName(string name)
        {
            Name = name;
        }

        public PSTypeName(Type type)
        {
            Type = type;
            Name = type.FullName;
        }

        //public PSTypeName(TypeDefinitionAst ast)
        //{
        //}

        //public PSTypeName(ITypeName typeName)
        //{
        //}

        public string Name { get; private set; }
        public Type Type { get; private set; }
        //public TypeDefinitionAst TypeDefinitionAst { get; }
    }
}

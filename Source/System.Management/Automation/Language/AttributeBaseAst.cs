// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public abstract class AttributeBaseAst : Ast
    {
        protected AttributeBaseAst(IScriptExtent extent, ITypeName typeName)
            : base(extent)
        {
            this.TypeName = typeName;
        }

        public ITypeName TypeName { get; private set; }
    }
}

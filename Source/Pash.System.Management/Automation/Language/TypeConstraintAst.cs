using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public class TypeConstraintAst : AttributeBaseAst
    {
        public TypeConstraintAst(IScriptExtent extent, ITypeName typeName)
            : base(extent, typeName)
        {
        }

        public TypeConstraintAst(IScriptExtent extent, Type type)
            : base(extent, /* TODO: */ null)
        {
            throw new NotImplementedException(this.ToString());
        }
    }
}

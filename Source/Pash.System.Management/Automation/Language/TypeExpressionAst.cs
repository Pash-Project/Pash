using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class TypeExpressionAst : ExpressionAst
    {
        public TypeExpressionAst(IScriptExtent extent, ITypeName typeName)
            : base(extent)
        {
            this.TypeName = typeName;
        }

        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }
        public ITypeName TypeName { get; private set; }

        public override string ToString()
        {
            return this.TypeName.ToString();
        }
    }
}

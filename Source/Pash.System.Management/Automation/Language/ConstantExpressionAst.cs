using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ConstantExpressionAst : ExpressionAst
    {
        public ConstantExpressionAst(IScriptExtent extent, object value)
            : base(extent)
        {
            this.Value = value;
        }

        // Derived classes should override this
        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }

        public object Value { get; private set; }
    }
}

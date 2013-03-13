using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ConvertExpressionAst : AttributedExpressionAst
    {
        public ConvertExpressionAst(IScriptExtent extent, TypeConstraintAst typeConstraint, ExpressionAst child)
            : base(extent, typeConstraint, child)
        {
            this.Type = typeConstraint;
        }

        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }
        public TypeConstraintAst Type { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Type;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }
    }
}

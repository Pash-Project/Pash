using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ExpandableStringExpressionAst : ExpressionAst
    {
        public ExpandableStringExpressionAst(IScriptExtent extent, string value, StringConstantType stringConstantType)
            : base(extent)
        {
            this.Value = value;
            this.StringConstantType = stringConstantType;
        }

        public ReadOnlyCollection<ExpressionAst> NestedExpressions { get { throw new NotImplementedException(this.ToString()); } }
        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }
        public StringConstantType StringConstantType { get; private set; }
        public string Value { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.NestedExpressions) yield return item;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }
    }
}

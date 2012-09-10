using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class BreakStatementAst : StatementAst
    {
        public BreakStatementAst(IScriptExtent extent, ExpressionAst label)
            : base(extent)
        {
            this.Label = label;
        }

        public ExpressionAst Label { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Label;
                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

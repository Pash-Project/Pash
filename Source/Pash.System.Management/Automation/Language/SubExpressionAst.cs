using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class SubExpressionAst : ExpressionAst
    {
        public SubExpressionAst(IScriptExtent extent, StatementBlockAst statementBlock)
            : base(extent)
        {
            this.SubExpression = statementBlock;
        }

        public StatementBlockAst SubExpression { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.SubExpression;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.SubExpression.ToString();
        }
    }
}

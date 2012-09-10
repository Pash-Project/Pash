using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class CommandExpressionAst : CommandBaseAst
    {
        public CommandExpressionAst(IScriptExtent extent, ExpressionAst expression, IEnumerable<RedirectionAst> redirections)
            : base(extent, redirections)
        {
            this.Expression = expression;
        }

        public ExpressionAst Expression { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Expression;
                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

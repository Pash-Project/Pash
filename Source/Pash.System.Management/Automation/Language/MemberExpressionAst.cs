using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class MemberExpressionAst : ExpressionAst
    {
        public MemberExpressionAst(IScriptExtent extent, ExpressionAst expression, CommandElementAst member, bool @static)
            : base(extent)
        {
            this.Expression = expression;
            this.Member = member;
            this.Static = @static;
        }

        public ExpressionAst Expression { get; private set; }
        public CommandElementAst Member { get; private set; }
        public bool Static { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Expression;
                yield return this.Member;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}::{1}", this.Expression, this.Member);
        }
    }
}

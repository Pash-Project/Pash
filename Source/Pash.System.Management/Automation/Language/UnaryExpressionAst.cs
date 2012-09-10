using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class UnaryExpressionAst : ExpressionAst
    {
        public UnaryExpressionAst(IScriptExtent extent, TokenKind tokenKind, ExpressionAst child)
            : base(extent)
        {
            this.TokenKind = tokenKind;
            this.Child = child;
        }
        
        public ExpressionAst Child { get; private set; }
        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }
        public TokenKind TokenKind { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Child;

                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

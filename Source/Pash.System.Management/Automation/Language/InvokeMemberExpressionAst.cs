using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Language
{
    public class InvokeMemberExpressionAst : MemberExpressionAst
    {
        public InvokeMemberExpressionAst(IScriptExtent extent, ExpressionAst expression, CommandElementAst method, IEnumerable<ExpressionAst> arguments, bool @static)
            : base(extent, expression, method, @static)
        {
            this.Arguments = arguments.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<ExpressionAst> Arguments { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Arguments) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

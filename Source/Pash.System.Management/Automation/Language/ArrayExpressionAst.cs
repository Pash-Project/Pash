using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ArrayExpressionAst : ExpressionAst
    {
        public ArrayExpressionAst(IScriptExtent extent, StatementBlockAst statementBlock)
            : base(extent)
        {
            this.SubExpression = statementBlock;
        }

        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }
        public StatementBlockAst SubExpression { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.SubExpression;
				foreach (var item in privateGetChildren()) yield return item;
			}
		}
		
		// Method call works around an issue compiling in mono
		private IEnumerable<Ast> privateGetChildren(){ return base.Children;}
	}
}

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public abstract class LoopStatementAst : LabeledStatementAst
    {
        protected LoopStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition)
        {
            this.Body = body;
        }

        public StatementBlockAst Body { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Body;
				foreach (var item in privateGetChildren()) yield return item;
			}
		}
		
		// Method call works around an issue compiling in mono
		private IEnumerable<Ast> privateGetChildren(){ return base.Children;}
	}
}

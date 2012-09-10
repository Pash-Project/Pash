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
                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

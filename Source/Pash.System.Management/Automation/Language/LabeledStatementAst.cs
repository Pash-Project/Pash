using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public abstract class LabeledStatementAst : StatementAst
    {
        protected LabeledStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition)
            : base(extent)
        {
            this.Label = label;
            this.Condition = condition;
        }

        public PipelineBaseAst Condition { get; private set; }
        public string Label { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Condition;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }
    }
}

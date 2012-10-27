using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public class ForStatementAst : LoopStatementAst
    {
        public ForStatementAst(IScriptExtent extent, string label, PipelineBaseAst initializer, PipelineBaseAst condition, PipelineBaseAst iterator, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
            this.Initializer = initializer;
            this.Iterator = iterator;
        }

        public PipelineBaseAst Initializer { get; private set; }
        public PipelineBaseAst Iterator { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Initializer;
                yield return this.Iterator;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("for ( {0} ; {1} ; {2} { ... }", this.Initializer, this.Condition, this.Iterator);
        }
    }
}

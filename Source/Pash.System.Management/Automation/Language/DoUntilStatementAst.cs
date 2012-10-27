using System;

namespace System.Management.Automation.Language
{
    public class DoUntilStatementAst : LoopStatementAst
    {
        public DoUntilStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
        }

        public override string ToString()
        {
            return string.Format("do { ... } until {0}", this.Condition);
        }
    }
}

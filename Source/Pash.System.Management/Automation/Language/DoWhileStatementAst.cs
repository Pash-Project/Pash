using System;

namespace System.Management.Automation.Language
{
    public class DoWhileStatementAst : LoopStatementAst
    {
        public DoWhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
        }

        public override string ToString()
        {
            return string.Format("do {{ ... }} while {0}", this.Condition);
        }
    }
}

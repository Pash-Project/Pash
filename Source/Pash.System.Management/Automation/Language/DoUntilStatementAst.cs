using System;

namespace System.Management.Automation.Language
{
    public class DoUntilStatementAst : LoopStatementAst
    {
        public DoUntilStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
        }
    }
}

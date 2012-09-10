using System;

namespace System.Management.Automation.Language
{
    public class WhileStatementAst : LoopStatementAst
    {
        public WhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
        }
    }
}

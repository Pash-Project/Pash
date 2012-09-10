using System;

namespace System.Management.Automation.Language
{
    public class DoWhileStatementAst : LoopStatementAst
    {
        public DoWhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Language
{
    public class WhileStatementAst : LoopStatementAst
    {
        public WhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition, body)
        {
        }

        public override string ToString()
        {
            return string.Format("while {0} { ... }", this.Condition.ToString());
        }
    }
}

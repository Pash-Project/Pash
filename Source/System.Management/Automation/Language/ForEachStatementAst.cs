// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public class ForEachStatementAst : LoopStatementAst
    {
        public ForEachStatementAst(IScriptExtent extent, string label, ForEachFlags flags, VariableExpressionAst variable, PipelineBaseAst expression, StatementBlockAst body)
            : base(extent, label, expression, body)
        {
            this.Flags = flags;
            this.Variable = variable;
        }

        public ForEachFlags Flags { get; private set; }
        public VariableExpressionAst Variable { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Variable;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("foreach ({0} in {1}) {{ ... }}", this.Variable, this.Condition);
        }
    }
}

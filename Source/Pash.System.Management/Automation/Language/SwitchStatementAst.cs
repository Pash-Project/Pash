// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class SwitchStatementAst : LabeledStatementAst
    {
        public SwitchStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, SwitchFlags flags, IEnumerable<Tuple<ExpressionAst, StatementBlockAst>> clauses, StatementBlockAst @default)
            : base(extent, label, condition)
        {
            this.Flags = flags;
            this.Clauses = clauses.ToReadOnlyCollection();
            this.Default = @default;
        }

        public ReadOnlyCollection<Tuple<ExpressionAst, StatementBlockAst>> Clauses { get; private set; }
        public StatementBlockAst Default { get; private set; }
        public SwitchFlags Flags { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                throw new NotImplementedException("verify behavior with PowerShell");
            }
        }

        public override string ToString()
        {
            return string.Format("switch ({0}) ...", this.Condition);
        }
    }
}

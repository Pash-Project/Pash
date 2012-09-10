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
                // A little odd. Verify what PowerShell does and write a test.
                foreach (var item in this.Clauses)
                {
                    yield return item.Item1;
                    yield return item.Item2;
                }

                yield return this.Default;

                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

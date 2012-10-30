using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class IfStatementAst : StatementAst
    {
        public IfStatementAst(IScriptExtent extent, IEnumerable<Tuple<PipelineBaseAst, StatementBlockAst>> clauses, StatementBlockAst elseClause)
            : base(extent)
        {
            this.Clauses = clauses.ToReadOnlyCollection();
            this.ElseClause = elseClause;
        }

        public ReadOnlyCollection<Tuple<PipelineBaseAst, StatementBlockAst>> Clauses { get; private set; }
        public StatementBlockAst ElseClause { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                throw new NotImplementedException("Verify with powershell");
            }
        }

        public override string ToString()
        {
            return string.Format("if {0} ...", this.Clauses[0]);
        }
    }
}

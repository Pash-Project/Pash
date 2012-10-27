using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class TryStatementAst : StatementAst
    {
        public TryStatementAst(IScriptExtent extent, StatementBlockAst body, IEnumerable<CatchClauseAst> catchClauses, StatementBlockAst @finally)
            : base(extent)
        {
            this.Body = body;
            this.CatchClauses = catchClauses.ToReadOnlyCollection();
            this.Finally = @finally;
        }

        public StatementBlockAst Body { get; private set; }
        public ReadOnlyCollection<CatchClauseAst> CatchClauses { get; private set; }
        public StatementBlockAst Finally { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Body;
                foreach (var item in this.CatchClauses) { yield return item; }
                yield return this.Finally;

                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return "try...";
        }
    }
}

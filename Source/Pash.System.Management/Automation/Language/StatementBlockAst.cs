using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class StatementBlockAst : Ast
    {
        public StatementBlockAst(IScriptExtent extent, IEnumerable<StatementAst> statements, IEnumerable<TrapStatementAst> traps)
            : base(extent)
        {
            this.Statements = statements.ToReadOnlyCollection();
            this.Traps = traps.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<StatementAst> Statements { get; private set; }
        public ReadOnlyCollection<TrapStatementAst> Traps { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Statements) yield return item;
                foreach (var item in this.Traps) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            if (this.Statements.Any())
            {
                return string.Format("{ {0} ...}", this.Statements.First());
            }
            else
            {
                return "{ }";
            }
        }
    }
}

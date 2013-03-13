using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    // http://msdn.microsoft.com/en-us/library/system.management.automation.language.namedblockast.namedblockast(v=vs.85).aspx
    public class NamedBlockAst : Ast
    {
        public NamedBlockAst(IScriptExtent extent, TokenKind blockName, StatementBlockAst statementBlock, bool unnamed)
            : base(extent)
        {
            this.BlockKind = blockName;
            if (statementBlock == null)
            {
                this.Statements = new ReadOnlyCollection<StatementAst>(new StatementAst[] { });
            }
            else
            {
                this.Statements = statementBlock.Statements;
            }
            this.Unnamed = unnamed;
        }

        public TokenKind BlockKind { get; private set; }
        public ReadOnlyCollection<StatementAst> Statements { get; private set; }
        //TODO: public ReadOnlyCollection<TrapStatementAst> Traps { get { throw new NotImplementedException(this.ToString()); } }
        public bool Unnamed { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Statements) yield return item;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }
    }
}

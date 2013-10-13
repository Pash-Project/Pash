// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;

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
                this.Traps = new ReadOnlyCollection<TrapStatementAst>(new TrapStatementAst[] { });
            }
            else
            {
                this.Statements = statementBlock.Statements;
                this.Traps = statementBlock.Traps;
            }
            this.Unnamed = unnamed;
        }

        public TokenKind BlockKind { get; private set; }
        public ReadOnlyCollection<StatementAst> Statements { get; private set; }
        public ReadOnlyCollection<TrapStatementAst> Traps { get; private set; }
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

        public override string ToString()
        {
            if (this.Statements.Any())
            {
                return "{0}: {{ {1} }}".FormatString(this.BlockKind, this.Statements.First());
            }
            else
            {
                return "{0}: {{ ... }}".FormatString(this.BlockKind);
            }
        }
    }
}

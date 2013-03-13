using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class DataStatementAst : StatementAst
    {
        public DataStatementAst(IScriptExtent extent, string variableName, IEnumerable<ExpressionAst> commandsAllowed, StatementBlockAst body)
            : base(extent)
        {
            this.Variable = variableName;
            this.CommandsAllowed = commandsAllowed.ToReadOnlyCollection();
            this.Body = body;
        }

        public StatementBlockAst Body { get; private set; }
        public ReadOnlyCollection<ExpressionAst> CommandsAllowed { get; private set; }
        public string Variable { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.CommandsAllowed) yield return item;
                yield return this.Body;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }
    }
}

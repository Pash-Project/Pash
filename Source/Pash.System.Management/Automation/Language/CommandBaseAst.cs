using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Management.Automation.Language
{
    public abstract class CommandBaseAst : StatementAst
    {
        protected CommandBaseAst(IScriptExtent extent, IEnumerable<RedirectionAst> redirections)
            : base(extent)
        {
            this.Redirections = redirections.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<RedirectionAst> Redirections { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Redirections) yield return item;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }
    }
}

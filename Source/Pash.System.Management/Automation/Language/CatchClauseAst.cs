using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class CatchClauseAst : Ast
    {
        public CatchClauseAst(IScriptExtent extent, IEnumerable<TypeConstraintAst> catchTypes, StatementBlockAst body)
            : base(extent)
        {
            this.CatchTypes = catchTypes.ToReadOnlyCollection();
            this.Body = body;
        }

        public StatementBlockAst Body { get; private set; }
        public ReadOnlyCollection<TypeConstraintAst> CatchTypes { get; private set; }
        public bool IsCatchAll { get { throw new NotImplementedException(this.ToString()); } }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.CatchTypes) yield return item;
                yield return this.Body;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }

    }
}

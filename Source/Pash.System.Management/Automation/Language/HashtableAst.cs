using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;

namespace System.Management.Automation.Language
{
    public class HashtableAst : ExpressionAst
    {
        public HashtableAst(IScriptExtent extent, IEnumerable<Tuple<ExpressionAst, StatementAst>> keyValuePairs)
            : base(extent)
        {
            this.KeyValuePairs = keyValuePairs.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<Tuple<ExpressionAst, StatementAst>> KeyValuePairs { get; private set; }
        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                // Examine how PowerShell does this.
                throw new NotImplementedException(this.ToString());
            }
        }

        public override string ToString()
        {
            return string.Format("@{ {0} ... }", this.KeyValuePairs.FirstOrDefault().ToString() ?? "<empty>");
        }
    }
}

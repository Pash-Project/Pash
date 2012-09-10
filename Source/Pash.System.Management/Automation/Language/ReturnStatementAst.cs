using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ReturnStatementAst : StatementAst
    {
        public ReturnStatementAst(IScriptExtent extent, PipelineBaseAst pipeline)
            : base(extent)
        {
            this.Pipeline = pipeline;
        }

        public PipelineBaseAst Pipeline { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Pipeline;
                foreach (var item in base.Children) yield return item;
            }
        }
    }
}

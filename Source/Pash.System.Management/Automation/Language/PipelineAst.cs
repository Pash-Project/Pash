using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class PipelineAst : PipelineBaseAst
    {
        public PipelineAst(IScriptExtent extent, CommandBaseAst commandAst)
            : base(extent)
        {
            this.PipelineElements = new[] { commandAst }.ToReadOnlyCollection();
        }

        public PipelineAst(IScriptExtent extent, IEnumerable<CommandBaseAst> pipelineElements)
            : base(extent)
        {
            this.PipelineElements = pipelineElements.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<CommandBaseAst> PipelineElements { get; private set; }

        public override ExpressionAst GetPureExpression() { throw new NotImplementedException(this.ToString()); }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.PipelineElements) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.PipelineElements[0].ToString();
        }
    }
}

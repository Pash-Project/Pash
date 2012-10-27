using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ScriptBlockExpressionAst : ExpressionAst
    {
        public ScriptBlockExpressionAst(IScriptExtent extent, ScriptBlockAst scriptBlock)
            : base(extent)
        {
            this.ScriptBlock = scriptBlock;
        }

        public ScriptBlockAst ScriptBlock { get; private set; }
        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.ScriptBlock;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.ScriptBlock.ToString();
        }
    }
}

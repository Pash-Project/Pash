// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class IndexExpressionAst : ExpressionAst
    {
        public IndexExpressionAst(IScriptExtent extent, ExpressionAst target, ExpressionAst index)
            : base(extent)
        {
            this.Target = target;
            this.Index = index;
        }

        public ExpressionAst Index { get; private set; }
        public ExpressionAst Target { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Target;
                yield return this.Index;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.Target, this.Index);
        }
    }
}

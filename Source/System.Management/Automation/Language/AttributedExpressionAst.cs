// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class AttributedExpressionAst : ExpressionAst
    {
        public AttributedExpressionAst(IScriptExtent extent, AttributeBaseAst attribute, ExpressionAst child)
            : base(extent)
        {
            this.Attribute = attribute;
            this.Child = child;
        }

        public AttributeBaseAst Attribute { get; private set; }
        public ExpressionAst Child { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Attribute;
                yield return this.Child;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.Attribute.ToString();
        }
    }
}

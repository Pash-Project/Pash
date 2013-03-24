// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class UsingExpressionAst : ExpressionAst
    {
        public UsingExpressionAst(IScriptExtent extent, ExpressionAst expressionAst)
            : base(extent)
        {
            this.SubExpression = expressionAst;
        }

        public ExpressionAst SubExpression { get; private set; }

        public static VariableExpressionAst ExtractUsingVariable(UsingExpressionAst usingExpressionAst)
        {
            throw new NotImplementedException(usingExpressionAst.ToString());
        }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.SubExpression;
                foreach (var item in base.Children) yield return item;
            }
        }
    
        public override string ToString()
        {
            return string.Format("using (0)", this.SubExpression);
        }
}
}

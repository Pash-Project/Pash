// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ConstantExpressionAst : ExpressionAst
    {
        public ConstantExpressionAst(IScriptExtent extent, object value)
            : base(extent)
        {
            this.Value = value;
        }

        public override Type StaticType { get { return this.Value.GetType(); } }

        public object Value { get; private set; }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}

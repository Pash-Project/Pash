// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;

namespace System.Management.Automation.Language
{
    public class VariableExpressionAst : ExpressionAst
    {
        public VariableExpressionAst(IScriptExtent extent, string variableName, bool splatted)
            : this(extent, new VariablePath(variableName), splatted)
        {
        }

        public VariableExpressionAst(IScriptExtent extent, VariablePath variablePath, bool splatted)
            : base(extent)
        {
            this.VariablePath = variablePath;
        }

        public bool Splatted { get; private set; }
        public VariablePath VariablePath { get; private set; }

        public bool IsConstantVariable() { throw new NotImplementedException(this.ToString()); }


        public override string ToString()
        {
            return string.Format("${0}", this.VariablePath.ToString());
        }
    }
}

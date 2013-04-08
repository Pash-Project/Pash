// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation.Language
{
    public abstract class PipelineBaseAst : StatementAst
    {
        protected PipelineBaseAst(IScriptExtent extent) : base(extent) { }

        public virtual ExpressionAst GetPureExpression() { throw new NotImplementedException(this.ToString()); }
    }
}

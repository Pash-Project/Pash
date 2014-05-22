// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation.Language
{
    public abstract class StatementAst : Ast
    {
        protected StatementAst(IScriptExtent extent) : base(extent)
        {
        }
    }
}

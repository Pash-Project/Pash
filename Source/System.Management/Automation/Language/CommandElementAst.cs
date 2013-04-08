// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
namespace System.Management.Automation.Language
{
    public abstract class CommandElementAst : Ast
    {
        protected CommandElementAst(IScriptExtent extent) : base(extent) { }
    }
}

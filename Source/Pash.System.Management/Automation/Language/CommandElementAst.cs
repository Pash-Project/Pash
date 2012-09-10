namespace System.Management.Automation.Language
{
    public abstract class CommandElementAst : Ast
    {
        protected CommandElementAst(IScriptExtent extent) : base(extent) { }
    }
}

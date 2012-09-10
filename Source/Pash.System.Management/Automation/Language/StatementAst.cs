namespace System.Management.Automation.Language
{
    public abstract class StatementAst : Ast
    {
        protected StatementAst(IScriptExtent extent) : base(extent) { }
    }
}

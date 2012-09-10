using System;

namespace System.Management.Automation.Language
{
    public abstract class ExpressionAst : CommandElementAst
    {
        protected ExpressionAst(IScriptExtent extent) : base(extent) { }

        public virtual Type StaticType { get { return typeof(object); } }
    }
}

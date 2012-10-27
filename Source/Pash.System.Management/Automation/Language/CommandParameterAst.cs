using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class CommandParameterAst : CommandElementAst
    {
        public CommandParameterAst(IScriptExtent extent, string parameterName, ExpressionAst argument, IScriptExtent errorPosition)
            : base(extent)
        {
            this.ParameterName = parameterName;
            this.Argument = argument;
            this.ErrorPosition = errorPosition;
        }

        public ExpressionAst Argument { get; private set; }
        public IScriptExtent ErrorPosition { get; private set; }
        public string ParameterName { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Argument;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("-{0} {1}", this.ParameterName, this.Argument);
        }
    }
}

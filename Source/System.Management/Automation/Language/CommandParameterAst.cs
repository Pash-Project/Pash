// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class CommandParameterAst : CommandElementAst
    {
        public CommandParameterAst(IScriptExtent extent, string parameterName, ExpressionAst argument, IScriptExtent errorPosition)
            : this(extent, parameterName, argument, errorPosition, false)
        {
        }

        internal CommandParameterAst(IScriptExtent extent, string parameterName, ExpressionAst argument,
                                     IScriptExtent errorPosition, bool requiresValue)
            : base(extent)
        {
            this.ParameterName = parameterName;
            this.Argument = argument;
            this.ErrorPosition = errorPosition;
            RequiresArgument = requiresValue;
        }

        public ExpressionAst Argument { get; internal set; }
        public IScriptExtent ErrorPosition { get; private set; }
        public string ParameterName { get; private set; }
        internal bool RequiresArgument { get; private set; }

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

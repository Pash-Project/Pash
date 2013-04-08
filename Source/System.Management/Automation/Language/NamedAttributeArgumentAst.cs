// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class NamedAttributeArgumentAst : Ast
    {
        public NamedAttributeArgumentAst(IScriptExtent extent, string argumentName, ExpressionAst argument, bool expressionOmitted)
            : base(extent)
        {
            this.Argument = argument;
            this.ArgumentName = argumentName;
            this.ExpressionOmitted = expressionOmitted;
        }

        public ExpressionAst Argument { get; private set; }
        public string ArgumentName { get; private set; }
        public bool ExpressionOmitted { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return Argument;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", this.ArgumentName, this.Argument);
        }
    }
}

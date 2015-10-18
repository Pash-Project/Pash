// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ConstantExpressionAst : ExpressionAst
    {
        public ConstantExpressionAst(IScriptExtent extent, object value)
            : base(extent)
        {
            this.Value = value;
        }

        public override Type StaticType { get { return this.Value.GetType(); } }

        public object Value { get; private set; }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        /// <summary>
        /// Member access expansion will be delayed when the constant literal is a part of command
        /// parameters. This property should return <c>true</c> for numeric constants so commands
        /// like <code>ping 8.8.8.8</code> will be processed correctly (it will be casted to
        /// string instead of treating it as a two member accessors a-la <code>(((8.8).8).8)</code>
        /// or subsequent numeric literals such as <code>8.8 .8 .8</code>.
        /// </summary>
        public virtual bool DelayMemberAccessExpansion
        {
            get { return true; }
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ParameterAst : Ast
    {
        public ParameterAst(IScriptExtent extent, VariableExpressionAst name, IEnumerable<AttributeBaseAst> attributes, ExpressionAst defaultValue)
            : base(extent)
        {
            this.Name = name;
            this.Attributes = attributes.ToReadOnlyCollection();
            this.DefaultValue = defaultValue;
        }

        public ReadOnlyCollection<AttributeBaseAst> Attributes { get; private set; }
        public ExpressionAst DefaultValue { get; private set; }
        public VariableExpressionAst Name { get; private set; }
        public Type StaticType { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Attributes) yield return item;
                yield return this.DefaultValue;
                yield return this.Name;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", this.Name, DefaultValue == null ? "" : DefaultValue.ToString());
        }
    }
}

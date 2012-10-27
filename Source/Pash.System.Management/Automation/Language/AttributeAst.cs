using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Language
{
    public class AttributeAst : AttributeBaseAst
    {
        public AttributeAst(IScriptExtent extent, ITypeName typeName, IEnumerable<ExpressionAst> positionalArguments, IEnumerable<NamedAttributeArgumentAst> namedArguments)
            : base(extent, typeName)
        {
            this.PositionalArguments = positionalArguments.ToReadOnlyCollection();
            this.NamedArguments = namedArguments.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<NamedAttributeArgumentAst> NamedArguments { get; private set; }
        public ReadOnlyCollection<ExpressionAst> PositionalArguments { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.NamedArguments) yield return item;
                foreach (var item in this.PositionalArguments) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.NamedArguments.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;

namespace System.Management.Automation.Language
{
    public class ParamBlockAst : Ast
    {
        public ParamBlockAst(IScriptExtent extent, IEnumerable<AttributeAst> attributes, IEnumerable<ParameterAst> parameters)
            : base(extent)
        {
            this.Attributes = attributes.ToReadOnlyCollection();
            this.Parameters = parameters.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<AttributeAst> Attributes { get; private set; }
        public ReadOnlyCollection<ParameterAst> Parameters { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Attributes) yield return item;
                foreach (var item in this.Parameters) yield return item;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("param { {0} ... }", this.Parameters.FirstOrDefault().ToString() ?? "<empty>");
        }
    }
}

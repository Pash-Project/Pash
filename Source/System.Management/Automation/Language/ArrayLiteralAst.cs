// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ArrayLiteralAst : ExpressionAst
    {
        public ArrayLiteralAst(IScriptExtent extent, IList<ExpressionAst> elements)
            : base(extent)
        {
            this.Elements = elements.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<ExpressionAst> Elements { get; private set; }
        public override Type StaticType { get { throw new NotImplementedException(this.ToString()); } }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.Elements) yield return item;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }

        public override string ToString()
        {
            return string.Format("{0}, ...", this.Elements.First());
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Pash;

namespace System.Management.Automation.Language
{
    public class ExpandableStringExpressionAst : ExpressionAst
    {
        public ExpandableStringExpressionAst(IScriptExtent extent, string value, StringConstantType stringConstantType)
            : base(extent)
        {
            this.Value = value;
            this.StringConstantType = stringConstantType;

            ParseExpandableString(value);
        }

        public ReadOnlyCollection<ExpressionAst> NestedExpressions { get; private set; }
        public override Type StaticType { get { return typeof(string); } }
        public StringConstantType StringConstantType { get; private set; }
        public string Value { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in this.NestedExpressions) yield return item;
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }

        public override string ToString()
        {
            switch (this.StringConstantType)
            {
                case StringConstantType.DoubleQuoted:
                    return string.Format("\"{0}\"", this.Value);

                case StringConstantType.DoubleQuotedHereString:
                    return string.Format("@\"{0}\"", this.Value);


                default:
                    throw new InvalidOperationException();
            }
        }

        private void ParseExpandableString(string value)
        {
            var parser = new ExpandableStringParser(Extent, value);
            parser.Parse();
            this.NestedExpressions = parser.NestedExpressions;
        }
    }
}

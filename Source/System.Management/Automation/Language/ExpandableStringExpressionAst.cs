// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Pash;

namespace System.Management.Automation.Language
{
    public class ExpandableStringExpressionAst : ExpressionAst
    {
        internal ExpandableStringExpressionAst(IScriptExtent extent, IList<ExpressionAst> expressions,
                                               string value, StringConstantType stringConstantType)
            : base(extent)
        {
            this.StringConstantType = stringConstantType;
            NestedExpressions = new ReadOnlyCollection<ExpressionAst>(expressions);
            Value = value;
        }


        public ExpandableStringExpressionAst(IScriptExtent extent, string value, StringConstantType stringConstantType)
            : this(extent, ParseExpandableString(extent, value), value, stringConstantType)
        {
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
            switch (StringConstantType)
            {
                case StringConstantType.DoubleQuoted:
                    return string.Format("\"{0}\"", this.Value);

                case StringConstantType.DoubleQuotedHereString:
                    return string.Format("@\"{0}\"", this.Value);

                default:
                    throw new InvalidOperationException();
            }
        }

        internal string ExpandString(IEnumerable<object> expandedValues)
        {
            expandedValues = expandedValues.Reverse();
            var exValEnumerator = expandedValues.GetEnumerator();

            var resultStr = new StringBuilder();
            string rest = Value;

            // we expand this string in reversed order so we don't need to adjust start indexes
            foreach (var item in NestedExpressions.Reverse())
            {
                if (!exValEnumerator.MoveNext())
                {
                    break;
                }
                // first find the relative position of the expandable part inside our string
                string value = LanguagePrimitives.ConvertTo<string>(exValEnumerator.Current);
                var relStart = item.Extent.StartOffset - Extent.StartOffset - 1;
                var relEnd = item.Extent.EndOffset - Extent.StartOffset - 1;
                // the end are constant words between this expression and the last resolved expression
                // we need to resolve escape constants before adding it
                var resolvedEnd = StringExpressionHelper.ResolveEscapeCharacters(rest.Substring(relEnd), StringConstantType);
                // as we do it in reverse order: insert at the beginning, in reverse order
                resultStr.Insert(0, resolvedEnd).Insert(0, value);
                // finally strip the rest which needs to be expanded
                rest = rest.Substring(0, relStart);
            }
            // now insert the rest at the beginning (other constant string)
            resultStr.Insert(0, StringExpressionHelper.ResolveEscapeCharacters(rest, StringConstantType));
            return resultStr.ToString();
        }

        private static IList<ExpressionAst> ParseExpandableString(IScriptExtent extent, string value)
        {
            var parser = new ExpandableStringParser(extent, value);
            parser.Parse();
            return parser.NestedExpressions;
        }

        private string ReplaceExpandableValue(string expStr, ExpressionAst ast, string value)
        {
            var relStart = ast.Extent.StartOffset - Extent.StartOffset - 1;
            var relEnd = ast.Extent.EndOffset - Extent.StartOffset - 1;
            return expStr.Substring(0, relStart) + value + expStr.Substring(relEnd);
        }
    }
}

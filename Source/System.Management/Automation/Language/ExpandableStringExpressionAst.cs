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
        public ExpandableStringExpressionAst(IScriptExtent extent, string value, StringConstantType stringConstantType)
            : base(extent)
        {
            this.StringConstantType = stringConstantType;
            ParseExpandableString(value);
        }

        public ReadOnlyCollection<ExpressionAst> NestedExpressions { get; private set; }
        public override Type StaticType { get { return typeof(string); } }
        public StringConstantType StringConstantType { get; private set; }

        private List<object> _stringParts = new List<object>();
        public string Value
        {
            get
            {
                return String.Join("", _stringParts);
            }
        }

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

            int currentIndex = 0;
            foreach (var nestedEx in NestedExpressions)
            {
                int nestedExpressionStartIndex = GetRelativeStartIndex(nestedEx);
                var strval = value.Substring(currentIndex, nestedExpressionStartIndex - currentIndex);
                if (strval.Length > 0)
                {
                    _stringParts.Add(StringExpressionHelper.ResolveEscapeCharacters(strval, StringConstantType));
                }
                _stringParts.Add(nestedEx);
                currentIndex = GetRelativeEndIndex(nestedEx);
            }
            if (currentIndex < value.Length)
            {
                _stringParts.Add(StringExpressionHelper.ResolveEscapeCharacters(value.Substring(currentIndex),
                                                                                StringConstantType));
            }
        }

        internal string ExpandString(IEnumerable<object> expandedValues)
        {
            var expandedString = new StringBuilder();
            var exValEnumerator = expandedValues.GetEnumerator();

            foreach (var strPart in _stringParts)
            {
                if (strPart is ExpressionAst && exValEnumerator.MoveNext())
                {
                    expandedString.Append(LanguagePrimitives.ConvertTo<string>(exValEnumerator.Current));
                }
                else
                {
                    expandedString.Append(strPart);
                }
            }
            return expandedString.ToString();
        }

        private int GetRelativeStartIndex(Ast ast)
        {
            return ast.Extent.StartOffset - Extent.StartOffset - 1;
        }

        private int GetRelativeEndIndex(Ast ast)
        {
            return ast.Extent.EndOffset - Extent.StartOffset - 1;
        }
    }
}

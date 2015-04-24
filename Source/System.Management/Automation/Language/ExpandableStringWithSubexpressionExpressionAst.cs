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
    public class ExpandableStringWithSubexpressionExpressionAst : ExpressionAst
    {
        public ExpandableStringWithSubexpressionExpressionAst(IScriptExtent extent, IEnumerable<Ast> expressions, string originalString, StringConstantType stringConstantType)
            : base(extent)
        {
            this.StringConstantType = stringConstantType;
            Expressions = expressions;
            OriginalString = originalString;
        }

        public string OriginalString;
        public IEnumerable<Ast> Expressions;

        public override Type StaticType { get { return typeof(string); } }
        public StringConstantType StringConstantType { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                foreach (var item in privateGetChildren()) yield return item;
            }
        }

        // Method call works around a Mono C# compiler crash
        [System.Diagnostics.DebuggerStepThrough]
        private IEnumerable<Ast> privateGetChildren() { return base.Children; }

        internal string ExpandString(IEnumerable<object> expandedValues)
        {
            expandedValues = expandedValues.Reverse();
            var exValEnumerator = expandedValues.GetEnumerator();

            string ret = OriginalString;
            int startPos = Extent.StartOffset;
            int endPos = Extent.EndOffset;

            foreach (var item in Expressions.Reverse())
            {
                if (item is StatementBlockAst)
                {
                    if (exValEnumerator.MoveNext())
                    {
                        int start = item.Extent.StartOffset - 2; // 2 = $(
                        int length = item.Extent.EndOffset + 1 - start; // 1 = )
                        ret = ret.Remove(start, length);
                        string value = LanguagePrimitives.ConvertTo<string>(exValEnumerator.Current);
                        ret = ret.Insert(start, value);

                        endPos = endPos - length + value.Length;
                    }
                }
                else if( item is ExpandableStringExpressionAst )
                {
                    if (exValEnumerator.MoveNext())
                    {
                        int start = item.Extent.StartOffset;
                        int length = item.Extent.EndOffset - start;
                        ret = ret.Remove(start, length);
                        string value = LanguagePrimitives.ConvertTo<string>(exValEnumerator.Current);
                        ret = ret.Insert(start, value);

                        endPos = endPos - length + value.Length;
                    }
                }
            }

            // Get our part (this expandable_string_with_subexpression) out of the original source text
            ret = ret.Substring(startPos, endPos - startPos);
            // Remove the start and end qoutes
            ret = ret.Substring(1, ret.Length - 2);
            // and lastly resolve any escaped qoutes
            ret = StringExpressionHelper.ResolveEscapeCharacters(ret, StringConstantType);
            return ret;
        }
    }
}

using System;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace System.Management.Automation
{
    // that class allows easy initialization
    class RegexReplacementTable : List<Tuple<Regex, string>>
    {
        public void Add(string regexPattern, string replacement)
        {
            Add(new Tuple<Regex, string>(new Regex(regexPattern), replacement));
        }
    }

    internal class StringExpressionHelper
    {
        private static RegexReplacementTable _escapeCharacterReplacements = new RegexReplacementTable() {
            {"(?<!`)`0", "\0"},
            {"(?<!`)`t", "\t"},
            {"(?<!`)`b", "\b"},
            {"(?<!`)`f", "\f"},
            {"(?<!`)`v", "\v"},
            {"(?<!`)`n", "\n"},
            {"(?<!`)`r", "\r"},
            {"(?<!`)`a", "\a"},
            {"(?<!`)`\'", "\'"},
            {"(?<!`)`\\$", "$"},
            {"(?<!`)\"\"", "\""},
            {"(?<!`)`\"", "\""},
            {"``", "`"}
        };

        public static  string ResolveEscapeCharacters(string orig, StringConstantType quoteType)
        {
            // look at this: http://technet.microsoft.com/en-us/library/hh847755.aspx
            // it's the about_Escape_Characters page which shows that escape characters are different in PS
            var value = orig;
            if (quoteType.Equals(StringConstantType.DoubleQuoted))
            {
                foreach (var tuple in _escapeCharacterReplacements)
                {
                    value = tuple.Item1.Replace(value, tuple.Item2);
                }
            }
            else if (quoteType.Equals(StringConstantType.SingleQuoted))
            {
                value = value.Replace("''", "'");
            }
            return value;
        }
    }
}


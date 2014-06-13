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
            {"([^`])(`0)", "$1\0"},
            {"([^`])(`t)", "$1\t"},
            {"([^`])(`b)", "$1\b"},
            {"([^`])(`f)", "$1\f"},
            {"([^`])(`v)", "$1\v"},
            {"([^`])(`n)", "$1\n"},
            {"([^`])(`r)", "$1\r"},
            {"([^`])(`a)", "$1\a"},
            {"([^`])(`\\$)", "$1$"},
            {"([^`])(\"\")", "$1\""},
            {"([^`])(`\")", "$1\""},
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


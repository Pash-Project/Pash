using System;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
    public sealed class WildcardPattern
    {
        private string _pattern;
        private Regex _patternRegEx;
        private WildcardOptions _options;

        public WildcardPattern(string pattern)
        {
            _pattern = pattern;
        }

        public WildcardPattern(string pattern, WildcardOptions options)
        {
            _pattern = pattern;
            _options = options;
        }

        public static bool ContainsWildcardCharacters(string pattern)
        {
            if ((pattern == null) || (pattern.Length == 0))
            {
                return false;
            }

            // TODO: deal with '[' & ']'
            return pattern.IndexOfAny(new char[] {'?', '*'}) != -1;
        }

        public static string Escape(string pattern) { throw new NotImplementedException(); }

        public bool IsMatch(string input)
        {
            if (Clear())
            {
                return _patternRegEx.IsMatch(input);
            }
            return false;
        }

        public static string Unescape(string pattern) { throw new NotImplementedException(); }

        private bool Clear()
        {
            if (_patternRegEx == null)
            {
                RegexOptions rexOpt = RegexOptions.Singleline;

                if ((_options & WildcardOptions.IgnoreCase) != WildcardOptions.None)
                {
                    rexOpt |= RegexOptions.IgnoreCase;
                }

                if ((_options & WildcardOptions.Compiled) != WildcardOptions.None)
                {
                    rexOpt |= RegexOptions.Compiled;
                }

                try
                {
                    _patternRegEx = new Regex(TranslateWildcardToRegex(_pattern), rexOpt);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    throw;
                }
            }
            return _patternRegEx != null;
        }

        private static string TranslateWildcardToRegex(string pattern)
        {
            if (pattern == null)
            {
                return null;
            }

            if (pattern.Length == 0)
                return ".*";

            // TODO: make this smarter / beef it up

            pattern = pattern.Replace(".", "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".?");

            return pattern;
        }
    }
}

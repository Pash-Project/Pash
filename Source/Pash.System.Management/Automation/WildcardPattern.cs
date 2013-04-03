// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        /// <summary>
        /// Checks if a given pattern is a wildcard.
        /// </summary>
        /// <param name="pattern">The pattern to check</param>
        /// <returns>True if it is, false if it isn't.</returns>
        public static bool ContainsWildcardCharacters(string pattern)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                return false;
            }

            return pattern.IndexOfAny(new char[] {'?', '*', '[', ']'}) != -1;
        }

        /// <summary>
        /// Converts a string with without characters to one with escape characters.
        /// </summary>
        /// <param name="pattern">The pattern to use.</param>
        /// <returns>The pattern with escape characters.</returns>
        public static string Escape(string pattern)
        {
            // Looks pretty, but could be made faster.
            return
                pattern.Replace("*", "`*")
                       .Replace("?", "`?")
                       .Replace("[", "`[")
                       .Replace("]", "`]");
        }

        /// <summary>
        /// Checks if a given input is a wildcard match.
        /// </summary>
        /// <param name="input">The given input.</param>
        /// <returns>True if it is, false if it isn't.</returns>
        public bool IsMatch(string input)
        {
            if (Clear())
            {
                return _patternRegEx.IsMatch(input);
            }
            return false;
        }

        /// <summary>
        /// Converts a string with escape characters to one without. Opposite of Escape()
        /// </summary>
        /// <param name="pattern">The pattern to use.</param>
        /// <returns>The pattern without escape characters.</returns>
        public static string Unescape(string pattern) 
        {
            // This block of code is actually about the same speed as a linear search because Mono (and probably .NET) seem to use the
            // Tuned Boyer-Moore text searching algorithm for Sting.Replace()
            return
                pattern.Replace("`*", "*")
                       .Replace("`?", "?")
                       .Replace("`[", "[")
                       .Replace("`]", "]");

        }

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

            pattern = pattern.Replace(".", "\\.")
                             .Replace("*", ".*")
                             .Replace("?", ".?");

            return pattern;
        }
    }
}

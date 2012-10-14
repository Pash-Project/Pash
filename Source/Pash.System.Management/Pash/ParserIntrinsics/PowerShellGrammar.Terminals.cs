using System;
using System.Collections.Generic;
using System.Linq;

using Irony.Parsing;
using System.Text.RegularExpressions;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Extensions.Reflection;

namespace Pash.ParserIntrinsics
{
    ////////
    // PowerShell Language Lexical Grammar, as presented in the PowerShell Language Specification[1], Appendix B.1
    //
    // [1]: http://www.microsoft.com/en_us/download/details.aspx?id=9706
    ///////

    partial class PowerShellGrammar
    {
        void InitializeTerminalFields()
        {
            var q = from field in typeof(PowerShellGrammar).GetFields()
                    where field.FieldType == typeof(RegexBasedTerminal)
                    select field;

            foreach (var field in q)
            {
                var patternFieldInfo = typeof(PowerShellGrammar).GetField(
                    field.Name + "_pattern",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    );
                var pattern = patternFieldInfo.GetValue<string>(null);
                var regexBasedTerminal = new RegexBasedTerminal(field.Name, pattern);

                field.SetValue(this, regexBasedTerminal);
            }

            // I'd rather that any other token be selected over `generic_token`, since it's, you know, generic.
            generic_token.Priority = TerminalPriority.Low - 1;

            // we want 'Get-ChildItem' to parse as a single token, not as 'Get - ChildItem'
            dash.Priority = TerminalPriority.Low;
        }

        #region B.1 Lexical grammar
        ////        input_elements:
        ////            input_element
        ////            input_elements   input_element
        ////        input_element:
        ////            whitespace
        ////            comment
        ////            token
        ////        input:
        ////            input_elements_opt   signature_block_opt
        ////        signature_block:
        ////            signature_begin   signature   signature_end
        ////        signature_begin:
        ////            new_line_character   # SIG # Begin signature block   new_line_character
        ////        signature:
        ////            base64 encoded signature blob in multiple single_line_comments
        ////        signature_end:
        ////            new_line_character   # SIG # End signature block   new_line_character

        #region B.1.1 Line terminators

        ////        new_line_character:
        ////            Carriage return character (U+000D)
        ////            Line feed character (U+000A)
        ////            Carriage return character (U+000D) followed by line feed character (U+000A)
        public readonly RegexBasedTerminal new_line_character = null; // Initialized by reflection.
        const string new_line_character_pattern = "(?<new_line_character>" + @"\u000D|\u000A|\u000D\u000A" + ")";
        const string new_line_character_ = @"\u000D\u000A";

        #endregion

        #region B.1.2 Comments
        void InitializeComments()
        {
            ////        comment:
            ////            single_line_comment
            ////            requires_comment
            ////            delimited_comment

            ////        single_line_comment:
            ////            #   input_characters_opt
            ////        input_characters:
            ////            input_character
            ////            input_characters   input_character
            ////        input_character:
            ////            Any Unicode character except a new_line_character
            CommentTerminal comment = new CommentTerminal("comment", "#", "\r", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);
        }


        ////        requires_comment:
        ////            #requires   whitespace   command_arguments

        ////        delimited_comment:
        ////            <#   delimited_comment_text_opt   hashes   >
        ////        delimited_comment_text:
        ////            delimited_comment_section
        ////            delimited_comment_text   delimited_comment_section
        ////        delimited_comment_section:
        ////            >
        ////            hashes_opt   not_greater_than_or_hash
        ////        hashes:
        ////            #
        ////            hashes   #
        ////        not_greater_than_or_hash:
        ////            Any Unicode character except > or #
        #endregion

        #region B.1.? dashes

        // this is in section B.1.2 in the spec, but it has nothing to do with comments
        ////        dash:
        ////            - (U+002D)
        ////            EnDash character (U+2013)
        ////            EmDash character (U+2014)
        ////            Horizontal bar character (U+2015)
        public readonly RegexBasedTerminal dash = null; // Initialized by reflection
        const string dash_pattern = "(?<dash>" + @"[\u002D\u2013\u2014\u2015]" + ")";

        ////        dashdash:
        ////            dash   dash
        public readonly RegexBasedTerminal dashdash = null; // Initialized by reflection
        const string dashdash_pattern = "(?<dashdash>" + dash_pattern + dash_pattern + ")";

        #endregion

        #region B.1.3 White space
        ////        whitespace:
        ////            Any character with Unicode class Zs, Zl, or Zp
        ////            Horizontal tab character (U+0009)
        ////            Vertical tab character (U+000B)
        ////            Form feed character (U+000C)
        ////            `   (The backtick character U+0060) followed by new_line_character
        // TODO: line continuation
        const string whitespace_ = @"\p{Zs}\p{Zl}\p{Zp}\u0009\u000B\u000C";
        public readonly RegexBasedTerminal whitespace = null; // Initialized by reflection.
        const string whitespace_pattern = "(?<whitespace>" + "[" + whitespace_ + "]" + ")";
        #endregion

        #region B.1.4 Tokens
        ////        token:
        ////            keyword
        ////            variable
        ////            command
        ////            command_parameter
        ////            command_argument_token
        ////            integer_literal
        ////            real_literal
        ////            string_literal
        ////            type_literal
        ////            operator_or_punctuator
        #endregion

        #region B.1.5 Keywords
        ////        keyword:  one of
        ////            begin				break			catch			class
        ////            continue			data			define		do
        ////            dynamicparam	else			elseif		end
        ////            exit				filter		finally		for
        ////            foreach			from			function		if
        ////            in					param			process		return
        ////            switch			throw			trap			try
        ////            until				using			var			while
        #endregion

        #region B.1.6 Variables
        ////        variable:
        ////            $$
        ////            $?
        ////            $^
        ////            $   variable_scope_opt  variable_characters
        ////            @   variable_scope_opt   variable_characters
        ////            braced_variable
        public readonly RegexBasedTerminal variable = null; // Initialized by reflection
        internal const string variable_pattern = "(?<variable>" +
            _variable_dollar_dollar_pattern + "|" +
            _variable_dollar_question_pattern + "|" +
            _variable_dollar_hat_pattern + "|" +
            _variable_ordinary_variable_pattern + "|" +
            _variable_splatted_variable_pattern + "|" +
            braced_variable_pattern +
            ")";

        // Not part of the original grammar, but helpful in making sense of this token tree.
        public readonly RegexBasedTerminal _variable_dollar_dollar = null; // Initialized by reflection
        internal const string _variable_dollar_dollar_pattern = "(?<_variable_dollar_dollar>" + @"\$\$" + ")";

        public readonly RegexBasedTerminal _variable_dollar_question = null; // Initialized by reflection
        internal const string _variable_dollar_question_pattern = "(?<_variable_dollar_question>" + @"\$\?" + ")";

        public readonly RegexBasedTerminal _variable_dollar_hat = null; // Initialized by reflection
        internal const string _variable_dollar_hat_pattern = "(?<_variable_dollar_hat>" + @"\$\^" + ")";

        public readonly RegexBasedTerminal _variable_ordinary_variable = null; // Initialized by reflection
        internal const string _variable_ordinary_variable_pattern = "(?<_variable_ordinary_variable>" + @"\$" + variable_scope_pattern + "?" + variable_characters_pattern + ")";

        public readonly RegexBasedTerminal _variable_splatted_variable = null; // Initialized by reflection
        internal const string _variable_splatted_variable_pattern = "(?<_variable_splatted_variable>" + @"\@" + variable_scope_pattern + "?" + variable_characters_pattern + ")";

        ////        braced_variable:
        ////            ${   variable_scope_opt   braced_variable_characters   }
        public readonly RegexBasedTerminal braced_variable = null; // Initialized by reflection
        const string braced_variable_pattern = "(?<braced_variable>" + @"\$\{" + braced_variable_characters_pattern + @"\}" + ")";

        ////        variable_scope:
        ////            global:
        ////            local:
        ////            private:
        ////            script:
        ////            variable_namespace
        public readonly RegexBasedTerminal variable_scope = null; // Initialized by reflection
        const string variable_scope_pattern = "(?<variable_scope>" + @"global\:|local\:|private\:|script\:|" + variable_namespace_pattern + ")";

        ////        variable_namespace:
        ////            variable_characters   :
        public readonly RegexBasedTerminal variable_namespace = null; // Initialized by reflection
        const string variable_namespace_pattern = "(?<variable_namespace>" + variable_characters_pattern + @"\:" + ")";

        ////        variable_characters:
        ////            variable_character
        ////            variable_characters   variable_character
        public readonly RegexBasedTerminal variable_characters = null; // Initialized by reflection
        const string variable_characters_pattern = "(?<variable_characters>" + variable_character_pattern + "+" + ")";

        ////        variable_character:
        ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
        ////            _   (The underscore character U+005F)
        ////            ?
        public readonly RegexBasedTerminal variable_character = null; // Initialized by reflection
        const string variable_character_pattern = "(?<variable_character>" + @"\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nd}|\u005F|\?" + ")";

        ////        braced_variable_characters:
        ////            braced_variable_character
        ////            braced_variable_characters   braced_variable_character
        public readonly RegexBasedTerminal braced_variable_characters = null; // Initialized by reflection
        const string braced_variable_characters_pattern = "(?<braced_variable_characters>" + braced_variable_character_pattern + "+" + ")";

        ////        braced_variable_character:
        ////            Any Unicode character except
        ////                    }   (The closing curly brace character U+007D)
        ////                    `   (The backtick character U+0060)
        ////            escaped_character
        public readonly RegexBasedTerminal braced_variable_character = null; // Initialized by reflection
        const string braced_variable_character_pattern = "(?<braced_variable_character>" + @"[^\u007D\0060]|" + escaped_character_pattern + ")";

        ////        escaped_character:
        ////            `   (The backtick character U+0060) followed by any Unicode character
        public readonly RegexBasedTerminal escaped_character = null; // Initialized by reflection
        const string escaped_character_pattern = "(?<escaped_character>" + @"\u0060." + ")";

        #endregion

        #region B.1.7 Commands
        ////        generic_token:
        ////            generic_token_parts
        public readonly RegexBasedTerminal generic_token = null; // Initialized by reflection.
        const string generic_token_pattern = "(?<generic_token>" + generic_token_parts_pattern + ")";

        ////        generic_token_parts:
        ////            generic_token_part
        ////            generic_token_parts   generic_token_part
        public readonly RegexBasedTerminal generic_token_parts = null; // Initialized by reflection.
        const string generic_token_parts_pattern = "(?<generic_token_parts>" + generic_token_part_pattern + "+" + ")";

        ////        generic_token_part:
        ////            expandable_string_literal
        ////            verbatim_here_string_literal
        ////            variable
        ////            generic_token_char
        public readonly RegexBasedTerminal generic_token_part = null; // Initialized by reflection.
        const string generic_token_part_pattern = "(?<generic_token_part>" + expandable_string_literal_pattern /*TODO: + "|" + verbatim_here_string_literal_pattern*/ + "|" + variable_pattern + "|" + generic_token_char_pattern + ")";

        ////        generic_token_char:
        ////            Any Unicode character except
        ////                    {		}		(		)		;		,		|		&		$
        ////                    `   (The backtick character U+0060)
        ////                    double_quote_character
        ////                    single_quote_character
        ////                    whitespace
        ////                    new_line_character
        ////:            escaped_character
        public readonly RegexBasedTerminal generic_token_char = null; // Initialized by reflection.
        const string generic_token_char_pattern = "(?<generic_token_char>" + "[^" + @"\{\}\(\)\;\,\|\&\$\u0060" + double_quote_character_ + single_quote_character_ + whitespace_ + new_line_character_ + "]|" + escaped_character_pattern + ")";

        ////        generic_token_with_subexpr_start:
        ////            generic_token_parts   $(
        public readonly RegexBasedTerminal generic_token_with_subexpr_start = null; // Initialized by reflection
        const string generic_token_with_subexpr_start_pattern = "(?<generic_token_with_subexpr_start>" + generic_token_parts_pattern + @"\$\(" + ")";

        ////        command_parameter:
        ////            dash   first_parameter_char   parameter_chars   colon_opt
        public readonly RegexBasedTerminal command_parameter = null; // Initialized by reflection
        const string command_parameter_pattern = "(?<command_parameter>" + dash_pattern + _parameter_name_pattern + colon_pattern + "?" + ")";

        public readonly RegexBasedTerminal _parameter_name = null; // Initialized by reflection
        const string _parameter_name_pattern = "(?<_parameter_name>" + first_parameter_char_pattern + parameter_chars_pattern + ")";

        ////        first_parameter_char:
        ////            A Unicode character of classes Lu, Ll, Lt, Lm, or Lo
        ////            _   (The underscore character U+005F)
        ////            ?
        public readonly RegexBasedTerminal first_parameter_char = null; // Initialized by reflection
        const string first_parameter_char_pattern = "(?<first_parameter_char>" + @"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\u005F\?]" + ")";

        ////        parameter_chars:
        ////            parameter_char
        ////            parameter_chars   parameter_char
        public readonly RegexBasedTerminal parameter_chars = null; // Initialized by reflection
        const string parameter_chars_pattern = "(?<parameter_chars>" + parameter_char_pattern + "+" + ")";

        ////        parameter_char:
        ////            Any Unicode character except
        ////                {	}	(	)	;	,	|	&	.	[
        ////                colon
        ////                whitespace
        ////                new_line_character
        public readonly RegexBasedTerminal parameter_char = null; // Initialized by reflection
        const string parameter_char_pattern = "(?<parameter_char>" + @"[^\{\}\(\)\;\,\|\&\.\[" + colon_ + whitespace_ + new_line_character_ + "]" + ")";

        ////        colon:
        ////            :   (The colon character U+003A)
        public readonly RegexBasedTerminal colon = null; // Initialized by reflection.
        const string colon_pattern = "(?<colon>" + colon_ + ")";
        const string colon_ = @"\u003A";
        #endregion

        #region B.1.8 Literals

        #region Integer Literals
        ////        decimal_integer_literal:
        ////            decimal_digits   numeric_type_suffix_opt   numeric_multiplier_opt
        public readonly RegexBasedTerminal decimal_integer_literal = null; // Initialized by reflection
        const string decimal_integer_literal_pattern = "(?<decimal_integer_literal>" + decimal_digits_pattern + numeric_type_suffix_pattern + "?" + numeric_multiplier_pattern + "?" + ")";

        ////        decimal_digits:
        ////            decimal_digit
        ////            decimal_digit   decimal_digits
        public readonly RegexBasedTerminal decimal_digits = null; // Initialized by reflection
        const string decimal_digits_pattern = "(?<decimal_digits>" + decimal_digit_pattern + "+" + ")";

        ////        decimal_digit:   one of
        ////            0   1   2   3   4   5   6   7   8   9
        public readonly RegexBasedTerminal decimal_digit = null; // Initialized by reflection
        const string decimal_digit_pattern = "(?<decimal_digit>" + "[0123456789]" + ")";

        ////        numeric_type_suffix:
        ////            long_type_suffix
        ////            decimal_type_suffix
        public readonly RegexBasedTerminal numeric_type_suffix = null; // Initialized by reflection
        const string numeric_type_suffix_pattern = "(?<numeric_type_suffix>" + long_type_suffix_pattern + "|" + decimal_type_suffix_pattern + ")";


        ////        hexadecimal_integer_literal:
        ////            0x   hexadecimal_digits   long_type_suffix_opt   numeric_multiplier_opt
        public readonly RegexBasedTerminal hexadecimal_integer_literal = null; // Initialized by reflection
        const string hexadecimal_integer_literal_pattern = "(?<hexadecimal_integer_literal>" + "0x" + hexadecimal_digits_pattern + long_type_suffix_pattern + "?" + numeric_multiplier_pattern + "?" + ")";

        ////        hexadecimal_digits:
        ////            hexadecimal_digit
        ////            hexadecimal_digit   decimal_digits
        public readonly RegexBasedTerminal hexadecimal_digits = null; // Initialized by reflection
        const string hexadecimal_digits_pattern = "(?<hexadecimal_digits>" + hexadecimal_digit_pattern + "+" + ")";

        ////        hexadecimal_digit:   one of
        ////            0   1   2   3   4   5   6   7   8   9   a   b   c   d   e   f
        public readonly RegexBasedTerminal hexadecimal_digit = null; // Initialized by reflection
        const string hexadecimal_digit_pattern = "(?<hexadecimal_digit>" + "[0-9a-f]" + ")";

        ////        long_type_suffix:
        ////            l
        public readonly RegexBasedTerminal long_type_suffix = null; // Initialized by reflection
        const string long_type_suffix_pattern = "(?<long_type_suffix>" + "l" + ")";

        ////        numeric_multiplier:   one of
        ////            kb   mb   gb   tb   pb
        public readonly RegexBasedTerminal numeric_multiplier = null; // Initialized by reflection
        const string numeric_multiplier_pattern = "(?<numeric_multiplier>" + "kb|mb|gb|tb|pb" + ")";

        #endregion

        #region Real Literals
        ////        real_literal:
        ////            decimal_digits   .   decimal_digits   exponent_part_opt   decimal_type_suffix_opt   numeric_multiplier_opt
        ////                             .   decimal_digits   exponent_part_opt   decimal_type_suffix_opt   numeric_multiplier_opt
        ////            decimal_digits                        exponent_part       decimal_type_suffix_opt   numeric_multiplier_opt
        public readonly RegexBasedTerminal real_literal = null; // Initialized by reflection
        const string real_literal_pattern = "(?<real_literal>" + _real_literal_1_pattern + "|" + _real_literal_2_pattern + "|" + _real_literal_3_pattern + ")";

        public readonly RegexBasedTerminal _real_literal_1 = null; // Initialized by reflection
        const string _real_literal_1_pattern = "(?<_real_literal_1>" + decimal_digits_pattern + @"\." + decimal_digits_pattern + exponent_part_pattern + "?" + decimal_type_suffix_pattern + "?" + numeric_multiplier_pattern + "?" + ")";

        public readonly RegexBasedTerminal _real_literal_2 = null; // Initialized by reflection
        const string _real_literal_2_pattern = "(?<_real_literal_2>" + @"\." + decimal_digits_pattern + exponent_part_pattern + "?" + decimal_type_suffix_pattern + "?" + numeric_multiplier_pattern + "?" + ")";

        public readonly RegexBasedTerminal _real_literal_3 = null; // Initialized by reflection
        const string _real_literal_3_pattern = "(?<_real_literal_3>" + decimal_digits_pattern + exponent_part_pattern + decimal_type_suffix_pattern + "?" + numeric_multiplier_pattern + "?" + ")";

        ////        exponent_part:
        ////            e   sign_opt   decimal_digits
        public readonly RegexBasedTerminal exponent_part = null; // Initialized by reflection
        const string exponent_part_pattern = "(?<exponent_part>" + "e" + sign_pattern + "?" + decimal_digit_pattern + ")";


        ////        sign:   one of
        ////            +
        ////            dash
        public readonly RegexBasedTerminal sign = null; // Initialized by reflection
        const string sign_pattern = "(?<sign>" + @"\+|" + dash_pattern + ")";


        ////        decimal_type_suffix:
        ////            d
        public readonly RegexBasedTerminal decimal_type_suffix = null; // Initialized by reflection
        const string decimal_type_suffix_pattern = "(?<decimal_type_suffix>" + "d" + ")";

        #endregion

        #region String Literals

        ////        expandable_string_literal:
        ////            double_quote_character   expandable_string_characters_opt   dollars_opt   double_quote_character
        public readonly RegexBasedTerminal expandable_string_literal = null; // Initialized by reflection.
        const string expandable_string_literal_pattern = "(?<expandable_string_literal>" + double_quote_character_pattern + expandable_string_characters_pattern + "?" + dollars_pattern + "?" + double_quote_character_pattern + ")";

        ////        double_quote_character:
        ////            "   (U+0022)
        ////            Left double quotation mark (U+201C)
        ////            Right double quotation mark (U+201D)
        ////            Double low_9 quotation mark (U+201E)
        public readonly RegexBasedTerminal double_quote_character = null; // Initialized by reflection.
        const string double_quote_character_pattern = "(?<double_quote_character>" + "[" + double_quote_character_ + "]" + ")";
        const string double_quote_character_ = @"\u0022\u201C\u201D\u201E";

        ////        expandable_string_characters:
        ////            expandable_string_part
        ////            expandable_string_characters   expandable_string_part
        public readonly RegexBasedTerminal expandable_string_characters = null; // Initialized by reflection.
        const string expandable_string_characters_pattern = "(?<expandable_string_characters>" + expandable_string_part_pattern + "+" + ")";

        ////        expandable_string_part:
        ////            Any Unicode character except
        ////                    $
        ////                    double_quote_character
        ////                    `   (The backtick character U+0060)
        ////            braced_variable
        ////            $   Any Unicode character except
        ////                    (
        ////                    {
        ////                    double_quote_character
        ////                    `   (The backtick character U+0060)
        ////            $   escaped_character
        ////            escaped_character
        ////            double_quote_character   double_quote_character
        public readonly RegexBasedTerminal expandable_string_part = null; // Initialized by reflection.
        const string expandable_string_part_pattern = "(?<expandable_string_part>" +
            _expandable_string_part_plain_pattern + "|" +
            _expandable_string_part_braced_variable_pattern + "|" +
            _expandable_string_part_expansion_pattern + "|" +
            _expandable_string_part_dollarescaped_pattern + "|" +
            _expandable_string_part_escaped_pattern + "|" +
            _expandable_string_part_quotequote_pattern + "|" +
            ")";

        public readonly RegexBasedTerminal _expandable_string_part_plain = null; // Initialized by reflection
        const string _expandable_string_part_plain_pattern = "(?<_expandable_string_part_plain>" + @"[^\$" + double_quote_character_ + @"\u0060]" + ")";

        public readonly RegexBasedTerminal _expandable_string_part_braced_variable = null; // Initialized by reflection
        const string _expandable_string_part_braced_variable_pattern = "(?<_expandable_string_part_braced_variable>" + braced_variable_character_pattern + ")";

        public readonly RegexBasedTerminal _expandable_string_part_expansion = null; // Initialized by reflection
        const string _expandable_string_part_expansion_pattern = "(?<_expandable_string_part_expansion>" + ")";

        public readonly RegexBasedTerminal _expandable_string_part_dollarescaped = null; // Initialized by reflection
        const string _expandable_string_part_dollarescaped_pattern = "(?<_expandable_string_part_dollarescaped>" + ")";

        public readonly RegexBasedTerminal _expandable_string_part_escaped = null; // Initialized by reflection
        const string _expandable_string_part_escaped_pattern = "(?<_expandable_string_part_escaped>" + ")";

        public readonly RegexBasedTerminal _expandable_string_part_quotequote = null; // Initialized by reflection
        const string _expandable_string_part_quotequote_pattern = "(?<_expandable_string_part_quotequote>" + ")";


        ////        dollars:
        ////            $
        ////            dollars   $
        public readonly RegexBasedTerminal dollars = null; // Initialized by reflection.
        const string dollars_pattern = "(?<dollars>" + @"\$+" + ")";

        ////        expandable_here_string_literal:
        ////            @   double_quote_character   whitespace_opt   new_line_character
        ////                    expandable_here_string_characters_opt   new_line_character   double_quote_character   @
        ////        expandable_here_string_characters:
        ////            expandable_here_string_part
        ////            expandable_here_string_characters   expandable_here_string_part
        ////        expandable_here_string_part:
        ////            Any Unicode character except
        ////                    $
        ////                    new_line_character
        ////            braced_variable
        ////            $   Any Unicode character except
        ////                    (
        ////                    new_line_character
        ////            $   new_line_character   Any Unicode character except double_quote_char
        ////            $   new_line_character   double_quote_char   Any Unicode character except @
        ////            new_line_character   Any Unicode character except double_quote_char
        ////            new_line_character   double_quote_char   Any Unicode character except @

        ////        expandable_string_with_subexpr_start:
        ////            double_quote_character   expandable_string_chars_opt [sic]   $(
        public readonly RegexBasedTerminal expandable_string_with_subexpr_start = null; // Initialized by reflection
        const string expandable_string_with_subexpr_start_pattern = "(?<expandable_string_with_subexpr_start>" + double_quote_character_pattern + expandable_string_characters_pattern + "?" + @"\$\(" + ")";

        ////        expandable_string_with_subexpr_end:
        ////            double_quote_char
        // Why does this exist as a distinct token? 
        public readonly RegexBasedTerminal expandable_string_with_subexpr_end = null; // Initialized by reflection
        const string expandable_string_with_subexpr_end_pattern = "(?<expandable_string_with_subexpr_end>" + double_quote_character_pattern + ")";

        ////        expandable_here_string_with_subexpr_start:
        ////            @   double_quote_character   whitespace_opt   new_line_character
        ////                    expandable_here_string_chars_opt   $(
        ////        expandable_here_string_with_subexpr_end:
        ////            new_line_character   double_quote_character   @

        ////        verbatim_string_literal:
        ////            single_quote_character   verbatim_string_characters_opt   single_quote_char [sic]
        public readonly RegexBasedTerminal verbatim_string_literal = null; // Initialized by reflection
        const string verbatim_string_literal_pattern = "(?<verbatim_string_literal>" + single_quote_character_pattern + verbatim_string_characters_pattern + "?" + single_quote_character_pattern + ")";

        ////        single_quote_character:
        ////            '   (U+0027)
        ////            Left single quotation mark (U+2018)
        ////            Right single quotation mark (U+2019)
        ////            Single low_9 quotation mark (U+201A)
        ////            Single high_reversed_9 quotation mark (U+201B)
        public readonly RegexBasedTerminal single_quote_character = null; // Initialized by reflection.
        const string single_quote_character_pattern = "(?<single_quote_character>" + "[" + single_quote_character_ + "]" + ")";
        const string single_quote_character_ = @"\u0027\u2018\u2019\u201A";

        ////        verbatim_string_characters:
        ////            verbatim_string_part
        ////            verbatim_string_characters   verbatim_string_part
        public readonly RegexBasedTerminal verbatim_string_characters = null; // Initialized by reflection.
        const string verbatim_string_characters_pattern = "(?<verbatim_string_characters>" + verbatim_string_part_pattern + "+" + ")";

        ////        verbatim_string_part:
        ////            Any Unicode character except single_quote_character
        ////            single_quote_character   single_quote_character
        public readonly RegexBasedTerminal verbatim_string_part = null; // Initialized by reflection
        const string verbatim_string_part_pattern = "(?<verbatim_string_part>" + "[^" + single_quote_character_ + "]|" + single_quote_character_ + single_quote_character_ + ")";

        ////        verbatim_here_string_literal:
        ////            @   single_quote_character   whitespace_opt   new_line_character
        ////                    verbatim_here_string_characters_opt   new_line_character   single_quote_character   @
        ////        verbatim_here_string_characters:
        ////            verbatim_here_string_part
        ////            verbatim_here_string_characters   verbatim_here_string_part
        ////        verbatim_here_string_part:
        ////            Any Unicode character except new_line_character
        ////            new_line_character   Any Unicode character except single_quote_character
        ////            new_line_character   single_quote_character   Any Unicode character except @
        #endregion
        #endregion

        #region B.1.9 Simple Names
        ////        simple_name:
        ////            simple_name_first_char   simple_name_chars
        ////        simple_name_chars:
        ////            simple_name_char
        ////            simple_name_chars   simple_name_char
        // The spec seems to require that simple names be at least 2 characters long but I think
        // single-characters simple names should be OK.
        public readonly RegexBasedTerminal simple_name = null; // Initialized by reflection
        const string simple_name_pattern = "(?<simple_name>" + simple_name_first_char_pattern + simple_name_char_pattern + "*" + ")";

        ////        simple_name_first_char:
        ////            A Unicode character of classes Lu, Ll, Lt, Lm, or Lo
        ////            _   (The underscore character U+005F)
        public readonly RegexBasedTerminal simple_name_first_char = null; // Initialized by reflection
        const string simple_name_first_char_pattern = "(?<simple_name_first_char>" + @"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\u005F]" + ")";

        ////        simple_name_char:
        ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
        ////            _   (The underscore character U+005F)
        public readonly RegexBasedTerminal simple_name_char = null; // Initialized by reflection
        const string simple_name_char_pattern = "(?<simple_name_char>" + @"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nd}\u005F]" + ")";

        #endregion

        #region B.1.10 Type Names
        ////        type_name:
        ////            type_identifier
        ////            type_name   .   type_identifier
        public readonly RegexBasedTerminal type_name = null; // Initialized by reflection
        const string type_name_pattern = "(?<type_name>" + type_identifier_pattern + "(" + @"\." + type_identifier_pattern + ")*" + ")";

        ////        type_identifier:
        ////            type_characters
        ////        type_characters:
        ////            type_character
        ////            type_characters   type_character
        public readonly RegexBasedTerminal type_identifier = null; // Initialized by reflection
        const string type_identifier_pattern = "(?<type_character>" + type_character_pattern + "+" + ")";

        ////        type_character:
        ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
        ////            _   (The underscore character U+005F)
        public readonly RegexBasedTerminal type_character = null; // Initialized by reflection
        const string type_character_pattern = "(?<type_character>" + @"\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nd}|\u005F" + ")";

        ////        array_type_name:
        ////            type_name   [
        public readonly RegexBasedTerminal array_type_name = null; // Initialized by reflection
        const string array_type_name_pattern = "(?<array_type_name>" + type_name_pattern + @"\[" + ")";

        ////        generic_type_name:
        ////            type_name   [
        public readonly RegexBasedTerminal generic_type_name = null; // Initialized by reflection
        const string generic_type_name_pattern = "(?<generic_type_name>" + type_name_pattern + @"\[" + ")";

        #endregion

        #region B.1.11 Operators and punctuators
        ////        operator_or_punctuator:  one of
        ////            {		}		[		]		(		)		@(		@{		$(		;
        ////            &&		||		&		|		,		++		..		::		.
        ////            !		*		/		%		+		2>&1	1>&2
        ////            dash					dash   dash
        ////            dash   and				dash   band				dash   bnot
        ////            dash   bor				dash   bxor				dash   not
        ////            dash   or				dash   xor
        ////            assignment_operator 
        ////            file_redirection_operator
        ////            comparison_operator
        ////            format_operator

        ////        assignment_operator:  one of
        ////            =		dash   =			+=		*=		/=		%=
        public readonly RegexBasedTerminal assignment_operator = null; // Initialized by reflection
        const string assignment_operator_pattern = "(?<assignment_operator>" + @"(\=)|(" + dash_pattern + @"\=)|(\+\=)|(\*\=)|(\/\=)|(\%\=)" + ")";

        ////        file_redirection_operator:  one of
        ////            >>		>		<		2>>		2>
        public readonly RegexBasedTerminal file_redirection_operator = null; // Initialized by reflection
        const string file_redirection_operator_pattern = "(?<file_redirection_operator>" + @"(\>\>)|" + @"(\>)|" + @"(\<)|" + @"(2\>\>)|" + @"(2\>)" + ")";

        ////        comparison_operator:  one of
        ////            dash   as					dash   ccontains				dash   ceq
        ////            dash   cge					dash   cgt						dash   cle
        ////            dash   clike				dash   clt						dash   cmatch
        ////            dash   cne					dash   cnotcontains				dash   cnotlike
        ////            dash   cnotmatch			dash   contains					dash   creplace
        ////            dash   csplit				dash   eq						dash   ge
        ////            dash   gt					dash   icontains				dash   ieq
        ////            dash   ige					dash   igt						dash   ile
        ////            dash   ilike				dash   ilt						dash   imatch
        ////            dash   ine					dash   inotcontains				dash   inotlike
        ////            dash   inotmatch			dash   ireplace					dash   is
        ////            dash   isnot				dash   isplit					dash   join
        ////            dash   le					dash   like						dash   lt
        ////            dash   match				dash   ne						dash   notcontains
        ////            dash   notlike				dash   notmatch					dash   replace
        ////            dash   split
        public readonly RegexBasedTerminal comparison_operator = null; // Initialized by reflection
        const string comparison_operator_pattern = "(?<comparison_operator>" + dash_pattern + @"
                                  as					| ccontains					| ceq 
                                | cge					| cgt						| cle
                                | clike					| clt						| cmatch
                                | cne					| cnotcontains				| cnotlike
                                | cnotmatch				| contains					| creplace
                                | csplit				| eq						| ge
                                | gt					| icontains					| ieq
                                | ige					| igt						| ile
                                | ilike					| ilt						| imatch
                                | ine					| inotcontains				| inotlike
                                | inotmatch				| ireplace					| is
                                | isnot					| isplit					| join
                                | le					| like						| lt
                                | match					| ne						| notcontains
                                | notlike				| notmatch					| replace
                                | split
"
            + ")";


        ////        format_operator:
        ////            dash   f
        public readonly RegexBasedTerminal format_operator = null; // Initialized by reflection
        const string format_operator_pattern = "(?<format_operator>" + dash_pattern + "f" + ")";

        #endregion
        #endregion

        // this appears to be missing from the language spec
        public readonly RegexBasedTerminal label = null; // Initialized by reflection
        const string label_pattern = "(?<label>" + simple_name_pattern + @"\:" + ")";

    }
}

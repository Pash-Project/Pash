using System;
using System.Collections.Generic;
using System.Linq;

using Irony.Parsing;
using System.Text.RegularExpressions;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Pash.ParserIntrinsics.Nodes;
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
        public static class Terminals
        {
            static Terminals()
            {
                var q = from field in typeof(PowerShellGrammar.Terminals).GetFields()
                        where field.FieldType == typeof(RegexBasedTerminal)
                        select field;

                foreach (var field in q)
                {
                    var patternFieldInfo = typeof(PowerShellGrammar.Terminals).GetField(
                        field.Name + "_pattern",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                        );
                    var pattern = patternFieldInfo.GetValue<string>(null);
                    var regexBasedTerminal = new RegexBasedTerminal(field.Name, pattern);

                    var nodeType = Assembly.GetCallingAssembly().GetType("Pash.ParserIntrinsics.Nodes." + field.Name + "_node");
                    if (nodeType == null)
                    {
                        regexBasedTerminal.Flags |= TermFlags.NoAstNode;
                    }
                    else
                    {
                        regexBasedTerminal.AstConfig.NodeType = nodeType;
                    }

                    field.SetValue(null, regexBasedTerminal);
                }

                // I'd rather that any other token be selected over `generic_token`, since it's, you know, generic.
                generic_token.Priority = -1;
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
            public static readonly RegexBasedTerminal new_line_character = null; // Initialized by reflection.
            const string new_line_character_pattern = "(?<new_line_character>" + @"(\u000D)|(\u000A)|(\u000D)(\u000A)" + ")";
            const string new_line_character_ = @"\u000D\u000A";

            ////        new_lines:
            ////            new_line_character
            ////            new_lines   new_line_character
            public static readonly RegexBasedTerminal new_lines = null; // Initialized by reflection.
            const string new_lines_pattern = "(?<new_lines>" + "(" + new_line_character_pattern + ")+" + ")";

            #endregion

            #region B.1.2 Comments
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
            ////        requires_comment:
            ////            #requires   whitespace   command_arguments
            ////        dash:
            ////            _ (U+002D)
            ////            EnDash character (U+2013)
            ////            EmDash character (U+2014)
            ////            Horizontal bar character (U+2015)
            ////        dashdash:
            ////            dash   dash
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

            #region B.1.3 White space
            ////        whitespace:
            ////            Any character with Unicode class Zs, Zl, or Zp
            ////            Horizontal tab character (U+0009)
            ////            Vertical tab character (U+000B)
            ////            Form feed character (U+000C)
            ////            `   (The backtick character U+0060) followed by new_line_character
            // TODO: line continuation
            const string whitespace_ = @"\p{Zs}\p{Zl}\p{Zp}\u0009\u000B\u000C";
            public static readonly RegexBasedTerminal whitespace = null; // Initialized by reflection.
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
            ////        braced_variable:
            ////            ${   variable_scope_opt   braced_variable_characters   }
            ////        variable_scope:
            ////        global:
            ////        local:
            ////        private:
            ////        script:
            ////            variable_namespace
            ////        variable_namespace:
            ////        variable_characters   :
            ////        variable_characters:
            ////            variable_character
            ////            variable_characters   variable_character
            ////        variable_character:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
            ////            _   (The underscore character U+005F)
            ////            ?
            ////        braced_variable_characters:
            ////            braced_variable_character
            ////            braced_variable_characters   braced_variable_character
            ////        braced_variable_character:
            ////            Any Unicode character except
            ////                    }   (The closing curly brace character U+007D)
            ////                    `   (The backtick character U+0060)
            ////            escaped_character
            ////        escaped_character:
            ////            `   (The backtick character U+0060) followed by any Unicode character
            #endregion

            #region B.1.7 Commands
            ////        generic_token:
            ////            generic_token_parts
            public static readonly RegexBasedTerminal generic_token = null; // Initialized by reflection.
            const string generic_token_pattern = "(?<generic_token>" + generic_token_parts_pattern + ")";

            ////        generic_token_parts:
            ////            generic_token_part
            ////            generic_token_parts   generic_token_part
            public static readonly RegexBasedTerminal generic_token_parts = null; // Initialized by reflection.
            const string generic_token_parts_pattern = "(?<generic_token_parts>" + "(" + generic_token_part_pattern + ")+" + ")";

            ////        generic_token_part:
            ////            expandable_string_literal
            ////            verbatim_here_string_literal
            ////            variable
            ////            generic_token_char
            // TODO: more
            public static readonly RegexBasedTerminal generic_token_part = null; // Initialized by reflection.
            const string generic_token_part_pattern = "(?<generic_token_part>" + generic_token_char_pattern + ")";

            ////        generic_token_char:
            ////            Any Unicode character except
            ////                    {		}		(		)		;		,		|		&		$
            ////                    `   (The backtick character U+0060)
            ////                    double_quote_character
            ////                    single_quote_character
            ////                    whitespace
            ////                    new_line_character
            ////TODO:            escaped_character
            public static readonly RegexBasedTerminal generic_token_char = null; // Initialized by reflection.
            const string generic_token_char_pattern = "(?<generic_token_char>" + "[^" + @"\{\}\(\)\;\,\|\&\$\u0060" + double_quote_character_ + single_quote_character_ + whitespace_ + new_line_character_ + "]" + ")";

            ////        generic_token_with_subexpr_start:
            ////            generic_token_parts   $(
            ////        command_parameter:
            ////            dash   first_parameter_char   parameter_chars   colon_opt
            ////        first_parameter_char:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, or Lo
            ////            _   (The underscore character U+005F)
            ////            ?
            ////        parameter_chars:
            ////            parameter_char
            ////            parameter_chars   parameter_char
            ////        parameter_char:
            ////            Any Unicode character except
            ////                {	}	(	)	;	,	|	&	.	[
            ////                colon
            ////                whitespace
            ////                new_line_character
            ////        colon:
            ////            :   (The colon character U+003A)
            public static readonly RegexBasedTerminal colon = null; // Initialized by reflection.
            const string colon_pattern = "(?<colon>" + @"\u003A" + ")";
            #endregion

            #region B.1.8 Literals

            #region Integer Literals
            ////        decimal_integer_literal:
            ////            decimal_digits   numeric_type_suffix_opt   numeric_multiplier_opt
            // TODO: more
            public static readonly RegexBasedTerminal decimal_integer_literal = null; // Initialized by reflection
            const string decimal_integer_literal_pattern = "(?<decimal_integer_literal>" + decimal_digits_pattern + ")";

            ////        decimal_digits:
            ////            decimal_digit
            ////            decimal_digit   decimal_digits
            public static readonly RegexBasedTerminal decimal_digits = null; // Initialized by reflection
            const string decimal_digits_pattern = "(?<decimal_digits>" + "(" + decimal_digit_pattern + ")+" + ")";

            ////        decimal_digit:   one of
            ////            0   1   2   3   4   5   6   7   8   9
            public static readonly RegexBasedTerminal decimal_digit = null; // Initialized by reflection
            const string decimal_digit_pattern = "(?<decimal_digit>" + "[0123456789]" + ")";

            ////        numeric_type_suffix:
            ////            long_type_suffix
            ////            decimal_type_suffix
            ////        hexadecimal_integer_literal:
            ////            0x   hexadecimal_digits   long_type_suffix_opt   numeric_multiplier_opt
            // TODO: more
            public static readonly RegexBasedTerminal hexadecimal_integer_literal = null; // Initialized by reflection
            const string hexadecimal_integer_literal_pattern = "(?<hexadecimal_integer_literal>" + "0x" + hexadecimal_digits_pattern + ")";

            ////        hexadecimal_digits:
            ////            hexadecimal_digit
            ////            hexadecimal_digit   decimal_digits
            public static readonly RegexBasedTerminal hexadecimal_digits = null; // Initialized by reflection
            const string hexadecimal_digits_pattern = "(?<hexadecimal_digits>" + "(" + hexadecimal_digit_pattern + ")+" + ")";

            ////        hexadecimal_digit:   one of
            ////            0   1   2   3   4   5   6   7   8   9   a   b   c   d   e   f
            public static readonly RegexBasedTerminal hexadecimal_digit = null; // Initialized by reflection
            const string hexadecimal_digit_pattern = "(?<hexadecimal_digit>" + "[0-9a-f]" + ")";

            ////        long_type_suffix:
            ////            l
            ////        numeric_multiplier:   one of
            ////            kb   mb   gb   tb   pb
            #endregion

            #region Real Literals
            ////        real_literal:
            ////            decimal_digits   .   decimal_digits   exponent_part_opt   decimal_type_suffix_opt   numeric_multiplier_opt
            ////            .   decimal_digits   exponent_part_opt   decimal_type_suffix_opt   numeric_multiplier_opt
            ////            decimal_digits   exponent_part  decimal_type_suffix_opt   numeric_multiplier_opt
            ////        exponent_part:
            ////            e   sign_opt   decimal_digits
            ////        sign:   one of
            ////            +
            ////            dash
            ////        decimal_type_suffix:
            ////            d
            #endregion

            #region String Literals

            ////        expandable_string_literal:
            ////            double_quote_character   expandable_string_characters_opt   dollars_opt   double_quote_character
            public static readonly RegexBasedTerminal expandable_string_literal = null; // Initialized by reflection.
            const string expandable_string_literal_pattern = "(?<expandable_string_literal>" + double_quote_character_pattern + "(" + expandable_string_characters_pattern + ")?(" + dollars_pattern + ")?" + double_quote_character_pattern + ")";

            ////        double_quote_character:
            ////            "   (U+0022)
            ////            Left double quotation mark (U+201C)
            ////            Right double quotation mark (U+201D)
            ////            Double low_9 quotation mark (U+201E)
            public static readonly RegexBasedTerminal double_quote_character = null; // Initialized by reflection.
            const string double_quote_character_pattern = "(?<double_quote_character>" + "[" + double_quote_character_ + "]" + ")";
            const string double_quote_character_ = @"\u0022\u201C\u201D\u201E";

            ////        expandable_string_characters:
            ////            expandable_string_part
            ////            expandable_string_characters   expandable_string_part
            public static readonly RegexBasedTerminal expandable_string_characters = null; // Initialized by reflection.
            const string expandable_string_characters_pattern = "(?<expandable_string_characters>" + "(" + expandable_string_part_pattern + ")+" + ")";

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
            // TODO: more
            public static readonly RegexBasedTerminal expandable_string_part = null; // Initialized by reflection.
            const string expandable_string_part_pattern = "(?<expandable_string_part>" + @"[^\$" + double_quote_character_ + @"\u0060]" + ")";

            ////        dollars:
            ////            $
            ////            dollars   $
            public static readonly RegexBasedTerminal dollars = null; // Initialized by reflection.
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
            ////            double_quote_character   expandable_string_chars_opt   $(
            ////        expandable_string_with_subexpr_end:
            ////            double_quote_char
            ////        expandable_here_string_with_subexpr_start:
            ////            @   double_quote_character   whitespace_opt   new_line_character
            ////                    expandable_here_string_chars_opt   $(
            ////        expandable_here_string_with_subexpr_end:
            ////            new_line_character   double_quote_character   @
            ////        verbatim_string_literal:
            ////            single_quote_character   verbatim_string_characters_opt   single_quote_char [sic]
            public static readonly RegexBasedTerminal verbatim_string_literal = null; // Initialized by reflection
            const string verbatim_string_literal_pattern = "(?<verbatim_string_literal>" + single_quote_character_pattern + "(" + verbatim_string_characters_pattern + ")?" + single_quote_character_pattern + ")";

            ////        single_quote_character:
            ////            '   (U+0027)
            ////            Left single quotation mark (U+2018)
            ////            Right single quotation mark (U+2019)
            ////            Single low_9 quotation mark (U+201A)
            ////            Single high_reversed_9 quotation mark (U+201B)
            public static readonly RegexBasedTerminal single_quote_character = null; // Initialized by reflection.
            const string single_quote_character_pattern = "(?<single_quote_character>" + "[" + single_quote_character_ + "]" + ")";
            const string single_quote_character_ = @"\u0027\u2018\u2019\u201A";

            ////        verbatim_string_characters:
            ////            verbatim_string_part
            ////            verbatim_string_characters   verbatim_string_part
            public static readonly RegexBasedTerminal verbatim_string_characters = null; // Initialized by reflection.
            const string verbatim_string_characters_pattern = "(?<verbatim_string_characters>" + "(" + verbatim_string_part_pattern + ")+" + ")";

            ////        verbatim_string_part:
            ////            Any Unicode character except single_quote_character
            ////            single_quote_character   single_quote_character
            public static readonly RegexBasedTerminal verbatim_string_part = null; // Initialized by reflection
            const string verbatim_string_part_pattern = "(?<verbatim_string_part>" + "([^" + single_quote_character_ + "])|(" + single_quote_character_ + single_quote_character_ + ")" + ")";

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
            ////        simple_name_first_char:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, or Lo
            ////            _   (The underscore character U+005F)
            ////        simple_name_chars:
            ////            simple_name_char
            ////            simple_name_chars   simple_name_char
            ////        simple_name_char:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
            ////            _   (The underscore character U+005F)
            #endregion

            #region B.1.10 Type Names
            ////        type_name:
            ////            type_identifier
            ////            type_name   .   type_identifier
            ////        type_identifier:
            ////            type_characters
            ////        type_characters:
            ////            type_character
            ////            type_characters   type_character
            ////        type_character:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
            ////            _   (The underscore character U+005F)
            ////        array_type_name:
            ////            type_name   [
            ////        generic_type_name:
            ////            type_name   [
            #endregion

            #region B.1.11 Operators and punctuators
            ////        operator_or_punctuator:  one of
            ////            {		}		[		]		(		)		@(		@{		$(		;
            ////        &&		||		&		|		,		++		..		::		.
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
            ////        file_redirection_operator:  one of
            ////            >>		>		<		2>>		2>
            ////        comparison_operator:  one of
            ////            dash   as					dash   ccontains				dash   ceq
            ////            dash   cge					dash   cgt						dash   cle
            ////            dash   clike				dash   clt						dash   cmatch
            ////            dash   cne					dash   cnotcontains			dash   cnotlike
            ////            dash   cnotmatch			dash   contains				dash   creplace
            ////            dash   csplit				dash   eq						dash   ge
            ////            dash   gt					dash   icontains				dash   ieq
            ////            dash   ige					dash   igt						dash   ile
            ////            dash   ilike				dash   ilt						dash   imatch
            ////            dash   ine					dash   inotcontains			dash   inotlike
            ////            dash   inotmatch			dash   ireplace				dash   is
            ////            dash   isnot				dash   isplit					dash   join
            ////            dash   le					dash   like						dash   lt
            ////            dash   match				dash   ne						dash   notcontains
            ////            dash   notlike				dash   notmatch				dash   replace
            ////            dash   split
            ////        format_operator:
            ////            dash   f
            #endregion
            #endregion
        }
    }
}

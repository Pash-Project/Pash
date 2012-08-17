using System;
using System.Collections.Generic;
using System.Linq;

using Irony.Parsing;
using System.Text.RegularExpressions;
using System.Text;

namespace ParserTests
{
    ////////
    // PowerShell Language Grammar, as presented in the PowerShell Language Specification[1], Appendix B.
    //
    // [1]: http://www.microsoft.com/en-us/download/details.aspx?id=9706
    ///////

    class PowerShellGrammar : CaseInsensitiveGrammar
    {
        static class LexicalPatterns
        {
            #region B.1 Lexical grammar
            ////        input-elements:
            ////            input-element
            ////            input-elements   input-element
            ////        input-element:
            ////            whitespace
            ////            comment
            ////            token
            const string input_elements = "(" + whitespace + ") | (" + token + ")";

            ////        input:
            ////            input-elements_opt   signature-block_opt
            // TODO: signature block
            const string input = "(" + input_elements + ")?";

            ////        signature-block:
            ////            signature-begin   signature   signature-end
            ////        signature-begin:
            ////            new-line-character   # SIG # Begin signature block   new-line-character
            ////        signature:
            ////            base64 encoded signature blob in multiple single-line-comments
            ////        signature-end:
            ////            new-line-character   # SIG # End signature block   new-line-character
            #region B.1.1 Line terminators
            ////        new-line-character:
            ////            Carriage return character (U+000D)
            ////            Line feed character (U+000A)
            ////            Carriage return character (U+000D) followed by line feed character (U+000A)
            const string new_line_character = "(\u000D)|(\u000A)|(\u000D)(\u000A)";

            ////        new-lines:
            ////            new-line-character
            ////            new-lines   new-line-character
            const string new_lines = "(" + new_line_character + ")+";

            #endregion
            #region B.1.2 Comments
            ////        comment:
            ////            single-line-comment
            ////            requires-comment
            ////            delimited-comment
            ////        single-line-comment:
            ////            #   input-characters_opt
            ////        input-characters:
            ////            input-character
            ////            input-characters   input-character
            ////        input-character:
            ////            Any Unicode character except a new-line-character
            ////        requires-comment:
            ////            #requires   whitespace   command-arguments
            ////        dash:
            ////            - (U+002D)
            ////            EnDash character (U+2013)
            ////            EmDash character (U+2014)
            ////            Horizontal bar character (U+2015)
            ////        dashdash:
            ////            dash   dash
            ////        delimited-comment:
            ////            <#   delimited-comment-text_opt   hashes   >
            ////        delimited-comment-text:
            ////            delimited-comment-section
            ////            delimited-comment-text   delimited-comment-section
            ////        delimited-comment-section:
            ////            >
            ////            hashes_opt   not-greater-than-or-hash
            ////        hashes:
            ////            #
            ////            hashes   #
            ////        not-greater-than-or-hash:
            ////            Any Unicode character except > or #
            #endregion
            #region B.1.3 White space
            ////        whitespace:
            ////            Any character with Unicode class Zs, Zl, or Zp
            ////            Horizontal tab character (U+0009)
            ////            Vertical tab character (U+000B)
            ////            Form feed character (U+000C)
            ////            `   (The backtick character U+0060) followed by new-line-character
            #endregion
            #region B.1.4 Tokens
            ////        token:
            ////            keyword
            ////            variable
            ////            command
            ////            command-parameter
            ////            command-argument-token
            ////            integer-literal
            ////            real-literal
            ////            string-literal
            ////            type-literal
            ////            operator-or-punctuator
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
            ////            $   variable-scope_opt  variable-characters
            ////            @   variable-scope_opt   variable-characters
            ////            braced-variable
            ////        braced-variable:
            ////            ${   variable-scope_opt   braced-variable-characters   }
            ////        variable-scope:
            ////        global:
            ////        local:
            ////        private:
            ////        script:
            ////            variable-namespace
            ////        variable-namespace:
            ////        variable-characters   :
            ////        variable-characters:
            ////            variable-character
            ////            variable-characters   variable-character
            ////        variable-character:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
            ////            _   (The underscore character U+005F)
            ////            ?
            ////        braced-variable-characters:
            ////            braced-variable-character
            ////            braced-variable-characters   braced-variable-character
            ////        braced-variable-character:
            ////            Any Unicode character except
            ////                    }   (The closing curly brace character U+007D)
            ////                    `   (The backtick character U+0060)
            ////            escaped-character
            ////        escaped-character:
            ////            `   (The backtick character U+0060) followed by any Unicode character
            #endregion
            #region B.1.7 Commands
            ////        generic-token:
            ////            generic-token-parts
            ////        generic-token-parts:
            ////            generic-token-part
            ////            generic-token-parts   generic-token-part
            ////        generic-token-part:
            ////            expandable-string-literal
            ////            verbatim-here-string-literal
            ////            variable
            ////            generic-token-char
            ////        generic-token-char:
            ////            Any Unicode character except
            ////                    {		}		(		)		;		,		|		&		$
            ////                    `   (The backtick character U+0060)
            ////                    double-quote-character
            ////                    single-quote-character
            ////                    whitespace
            ////                    new-line-character
            ////            escaped-character
            ////        generic-token-with-subexpr-start:
            ////            generic-token-parts   $(
            ////        command-parameter:
            ////            dash   first-parameter-char   parameter-chars   colon_opt
            ////        first-parameter-char:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, or Lo
            ////            _   (The underscore character U+005F)
            ////            ?
            ////        parameter-chars:
            ////            parameter-char
            ////            parameter-chars   parameter-char
            ////        parameter-char:
            ////            Any Unicode character except
            ////                {	}	(	)	;	,	|	&	.	[
            ////                colon
            ////                whitespace
            ////                new-line-character
            ////        colon:
            ////        :   (The colon character U+003A)
            #endregion
            #region B.1.8 Literals
            ////        literal:
            ////            integer-literal
            ////            real-literal
            ////            string-literal
            ////            Integer Literals
            ////        integer-literal:
            ////            decimal-integer-literal
            ////            hexadecimal-integer-literal
            ////        decimal-integer-literal:
            ////            decimal-digits   numeric-type-suffix_opt   numeric-multiplier_opt
            ////        decimal-digits:
            ////            decimal-digit
            ////            decimal-digit   decimal-digits
            ////        decimal-digit:   one of
            ////            0   1   2   3   4   5   6   7   8   9
            ////        numeric-type-suffix:
            ////            long-type-suffix
            ////            decimal-type-suffix
            ////        hexadecimal-integer-literal:
            ////            0x   hexadecimal-digits   long-type-suffix_opt   numeric-multiplier_opt
            ////        hexadecimal-digits:
            ////            hexadecimal-digit
            ////            hexadecimal-digit   decimal-digits
            ////        hexadecimal-digit:   one of
            ////            0   1   2   3   4   5   6   7   8   9   a   b   c   d   e   f
            ////        long-type-suffix:
            ////            l
            ////        numeric-multiplier:   one of
            ////            kb   mb   gb   tb   pb
            ////            Real Literals
            ////        real-literal:
            ////            decimal-digits   .   decimal-digits   exponent-part_opt   decimal-type-suffix_opt   numeric-multiplier_opt
            ////            .   decimal-digits   exponent-part_opt   decimal-type-suffix_opt   numeric-multiplier_opt
            ////            decimal-digits   exponent-part  decimal-type-suffix_opt   numeric-multiplier_opt
            ////        exponent-part:
            ////            e   sign_opt   decimal-digits
            ////        sign:   one of
            ////            +
            ////            dash
            ////        decimal-type-suffix:
            ////            d
            ////            String Literals
            ////        string-literal:
            ////            expandable-string-literal
            ////            expandable-here-string-literal
            ////            verbatim-string-literal
            ////            verbatim-here-string-literal
            ////        expandable-string-literal:
            ////            double-quote-character   expandable-string-characters_opt   dollars_opt   double-quote-character
            ////        double-quote-character:
            ////            "   (U+0022)
            ////            Left double quotation mark (U+201C)
            ////            Right double quotation mark (U+201D)
            ////            Double low-9 quotation mark (U+201E)
            ////        expandable-string-characters:
            ////            expandable-string-part
            ////            expandable-string-characters   expandable-string-part
            ////        expandable-string-part:
            ////            Any Unicode character except
            ////                    $
            ////                    double-quote-character
            ////                    `   (The backtick character U+0060)
            ////            braced-variable
            ////            $   Any Unicode character except
            ////                    (
            ////                    {
            ////                    double-quote-character
            ////                    `   (The backtick character U+0060)
            ////            $   escaped-character
            ////            escaped-character
            ////            double-quote-character   double-quote-character
            ////        dollars:
            ////            $
            ////            dollars   $
            ////        expandable-here-string-literal:
            ////            @   double-quote-character   whitespace_opt   new-line-character
            ////                    expandable-here-string-characters_opt   new-line-character   double-quote-character   @
            ////        expandable-here-string-characters:
            ////            expandable-here-string-part
            ////            expandable-here-string-characters   expandable-here-string-part
            ////        expandable-here-string-part:
            ////            Any Unicode character except
            ////                    $
            ////                    new-line-character
            ////            braced-variable
            ////            $   Any Unicode character except
            ////                    (
            ////                    new-line-character
            ////            $   new-line-character   Any Unicode character except double-quote-char
            ////            $   new-line-character   double-quote-char   Any Unicode character except @
            ////            new-line-character   Any Unicode character except double-quote-char
            ////            new-line-character   double-quote-char   Any Unicode character except @
            ////        expandable-string-with-subexpr-start:
            ////            double-quote-character   expandable-string-chars_opt   $(
            ////        expandable-string-with-subexpr-end:
            ////            double-quote-char
            ////        expandable-here-string-with-subexpr-start:
            ////            @   double-quote-character   whitespace_opt   new-line-character
            ////                    expandable-here-string-chars_opt   $(
            ////        expandable-here-string-with-subexpr-end:
            ////            new-line-character   double-quote-character   @
            ////        verbatim-string-literal:
            ////            single-quote-character   verbatim-string-characters_opt   single-quote-char
            ////        single-quote-character:
            ////            '   (U+0027)
            ////            Left single quotation mark (U+2018)
            ////            Right single quotation mark (U+2019)
            ////            Single low-9 quotation mark (U+201A)
            ////            Single high-reversed-9 quotation mark (U+201B)
            ////        verbatim-string-characters:
            ////            verbatim-string-part
            ////            verbatim-string-characters   verbatim-string-part
            ////        verbatim-string-part:
            ////            Any Unicode character except single-quote-character
            ////            single-quote-character   single-quote-character
            ////        verbatim-here-string-literal:
            ////            @   single-quote-character   whitespace_opt   new-line-character
            ////                    verbatim-here-string-characters_opt   new-line-character   single-quote-character   @
            ////        verbatim-here-string-characters:
            ////            verbatim-here-string-part
            ////            verbatim-here-string-characters   verbatim-here-string-part
            ////        verbatim-here-string-part:
            ////            Any Unicode character except new-line-character
            ////            new-line-character   Any Unicode character except single-quote-character
            ////            new-line-character   single-quote-character   Any Unicode character except @
            #endregion
            #region B.1.9 Simple Names
            ////        simple-name:
            ////            simple-name-first-char   simple-name-chars
            ////        simple-name-first-char:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, or Lo
            ////            _   (The underscore character U+005F)
            ////        simple-name-chars:
            ////            simple-name-char
            ////            simple-name-chars   simple-name-char
            ////        simple-name-char:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
            ////            _   (The underscore character U+005F)
            #endregion
            #region B.1.10 Type Names
            ////        type-name:
            ////            type-identifier
            ////            type-name   .   type-identifier
            ////        type-identifier:
            ////            type-characters
            ////        type-characters:
            ////            type-character
            ////            type-characters   type-character
            ////        type-character:
            ////            A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nd
            ////            _   (The underscore character U+005F)
            ////        array-type-name:
            ////            type-name   [
            ////        generic-type-name:
            ////            type-name   [
            #endregion
            #region B.1.11 Operators and punctuators
            ////        operator-or-punctuator:  one of
            ////            {		}		[		]		(		)		@(		@{		$(		;
            ////        &&		||		&		|		,		++		..		::		.
            ////            !		*		/		%		+		2>&1	1>&2
            ////            dash					dash   dash
            ////            dash   and				dash   band				dash   bnot
            ////            dash   bor				dash   bxor				dash   not
            ////            dash   or				dash   xor
            ////            assignment-operator 
            ////            file-redirection-operator
            ////            comparison-operator
            ////            format-operator
            ////        assignment-operator:  one of
            ////            =		dash   =			+=		*=		/=		%=
            ////        file-redirection-operator:  one of
            ////            >>		>		<		2>>		2>
            ////        comparison-operator:  one of
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
            ////        format-operator:
            ////            dash   f
            #endregion
            #endregion
        }

        public PowerShellGrammar()
        {
            #region B.2 Syntactic grammar
            #region B.2.1 Basic concepts
            ////        script-file:
            ////            script-block
            ////        module-file:
            ////            script-block
            ////        interactive-input:
            ////            script-block
            ////        data-file:
            ////            statement-list
            #endregion
            #region B.2.2 Statements
            ////        script-block:
            ////            param-block_opt   statement-terminators_opt    script-block-body_opt
            ////        param-block:
            ////            new-lines_opt   attribute-list_opt   new-lines_opt   param   new-lines_opt
            ////                    (   parameter-list_opt   new-lines_opt   )
            ////        parameter-list:
            ////            script-parameter
            ////            parameter-list   new-lines_opt   ,   script-parameter
            ////        script-parameter:
            ////            new-lines_opt   attribute-list_opt   new-lines_opt   variable   script-parameter-default_opt
            ////        script-parameter-default:
            ////            new-lines_opt   =   new-lines_opt   expression
            ////        script-block-body:
            ////            named-block-list
            ////            statement-list
            ////        named-block-list:
            ////            named-block
            ////            named-block-list   named-block
            ////        named-block:
            ////            block-name   statement-block   statement-terminators_opt
            ////        block-name:  one of
            ////            dynamicparam   begin   process   end
            ////        statement-block:
            ////            new-lines_opt   {   statement-list_opt   new-lines_opt   }
            ////        statement-list:
            ////            statement
            ////            statement-list   statement
            ////        statement:
            ////            if-statement
            ////            label_opt   labeled-statement
            ////            function-statement
            ////            flow-control-statement   statement-terminator
            ////            trap-statement
            ////            try-statement
            ////            data-statement
            ////            pipeline   statement-terminator
            ////        statement-terminator:
            ////            ;
            ////            new-line-character
            ////        statement-terminators:
            ////            statement-terminator
            ////            statement-terminators   statement-terminator
            ////        if-statement:
            ////            if   new-lines_opt   (   new-lines_opt   pipeline   new-lines_opt   )   statement-block
            ////                     elseif-clauses_opt   else-clause_opt
            ////        elseif-clauses:
            ////            elseif-clause
            ////            elseif-clauses   elseif-clause
            ////        elseif-clause:
            ////            new-lines_opt   elseif   new-lines_opt   (   new-lines_opt   pipeline   new-lines_opt   )   statement-block
            ////        else-clause:
            ////            new-lines_opt   else   statement-block
            ////        labeled-statement:
            ////            switch-statement
            ////            foreach-statement
            ////            for-statement
            ////            while-statement
            ////            do-statement
            ////        switch-statement:
            ////            switch   new-lines_opt   switch-parameters_opt   switch-condition   switch-body
            ////        switch-parameters:
            ////            switch-parameter
            ////            switch-parameters   switch-parameter
            ////        switch-parameter:
            ////            -regex
            ////            -wildcard
            ////            -exact
            ////            -casesensitive
            ////        switch-condition:
            ////            (   new-lines_opt   pipeline   new-lines_opt   )
            ////            -file   new-lines_opt   switch-filename
            ////        switch-filename:
            ////            command-argument
            ////            primary-expression
            ////        switch-body:
            ////            new-lines_opt   {   new-lines_opt   switch-clauses   }
            ////        switch-clauses:
            ////            switch-clause
            ////            switch-clauses   switch-clause
            ////        switch-clause:
            ////            switch-clause-condition   statement-block   statement-terimators_opt
            ////        switch-clause-condition:
            ////            command-argument
            ////            primary-expression
            ////        foreach-statement:
            ////            foreach   new-lines_opt   (   new-lines_opt   variable   new-lines_opt   in   new-lines_opt   pipeline
            ////                    new-lines_opt   )   statement-block
            ////        for-statement:
            ////            for   new-lines_opt   (
            ////                    new-lines_opt   for-initializer_opt   statement-terminator
            ////                    new-lines_opt   for-condition_opt   statement-terminator
            ////                    new-lines_opt   for-iterator_opt
            ////                    new-lines_opt   )   statement-block
            ////            for   new-lines_opt   (
            ////                    new-lines_opt   for-initializer_opt   statement-terminator
            ////                    new-lines_opt   for-condition_opt
            ////                    new-lines_opt   )   statement-block
            ////            for   new-lines_opt   (
            ////                    new-lines_opt   for-initializer_opt
            ////                    new-lines_opt   )   statement-block
            ////        for-initializer:
            ////            pipeline
            ////        for-condition:
            ////            pipeline
            ////        for-iterator:
            ////            pipeline
            ////        while-statement:
            ////            while   new-lines_opt   (   new-lines_opt   while-condition   new-lines_opt   )   statement-block
            ////        do-statement:
            ////            do   statement-block  new-lines_opt   while   new-lines_opt   (   while-condition   new-lines_opt   )
            ////            do   statement-block   new-lines_opt   until   new-lines_opt   (   while-condition   new-lines_opt   )
            ////        while-condition:
            ////            new-lines_opt   pipeline
            ////        function-statement:
            ////            function   new-lines_opt   function-name   function-parameter-declaration_opt   {   script-block   }
            ////            filter   new-lines_opt   function-name   function-parameter-declaration_opt   {   script-block   }
            ////        function-name:
            ////            command-argument
            ////        function-parameter-declaration:
            ////            new-lines_opt   (   parameter-list   new-lines_opt   )
            ////        flow-control-statement:
            ////            break   label-expression_opt
            ////            continue   label-expression_opt
            ////            throw    pipeline_opt
            ////            return   pipeline_opt
            ////            exit   pipeline_opt
            ////        label-expression:
            ////            simple-name
            ////            unary-expression
            ////        trap-statement:
            ////            trap  new-lines_opt   type-literal_opt   new-lines_opt   statement-block
            ////        try-statement:
            ////            try   statement-block   catch-clauses
            ////            try   statement-block   finally-clause
            ////            try   statement-block   catch-clauses   finally-clause
            ////        catch-clauses:
            ////            catch-clause
            ////            catch-clauses   catch-clause
            ////        catch-clause:
            ////            new-lines_opt   catch   catch-type-list_opt   statement-block
            ////        catch-type-list:
            ////            new-lines_opt   type-literal
            ////            catch-type-list   new-lines_opt   ,   new-lines_opt   type-literal
            ////        finally-clause:
            ////            new-lines_opt   finally   statement-block
            ////        data-statement:
            ////            data    new-lines_opt   data-name   data-commands-allowed_opt   statement-block
            ////        data-name:
            ////            simple-name
            ////        data-commands-allowed:
            ////            new-lines_opt   -supportedcommand   data-commands-list
            ////        data-commands-list:
            ////            new-lines_opt   data-command
            ////            data-commands-list   ,   new-lines_opt   data-command
            ////        data-command:
            ////            command-name-expr
            ////        pipeline:
            ////            assignment-expression
            ////            expression   redirections_opt  pipeline-tail_opt
            ////            command   pipeline-tail_opt
            ////        assignment-expression:
            ////            expression   assignment-operator   statement
            ////        pipeline-tail:
            ////            |   new-lines_opt   command
            ////            |   new-lines_opt   command   pipeline-tail
            ////        command:
            ////            command-name   command-elements_opt
            ////            command-invocation-operator   command-module_opt  command-name-expr   command-elements_opt
            ////        command-invocation-operator:  one of
            ////            &	.
            ////        command-module:
            ////            primary-expression
            ////        command-name:
            ////            generic-token
            ////            generic-token-with-subexpr
            ////        generic-token-with-subexpr:
            ////            No whitespace is allowed between ) and command-name.
            ////            generic-token-with-subexpr-start   statement-list_opt   )   command-name
            ////        command-name-expr:
            ////            command-name
            ////            primary-expression
            ////        command-elements:
            ////            command-element
            ////            command-elements   command-element
            ////        command-element:
            ////            command-parameter
            ////            command-argument
            ////            redirection
            ////        command-argument:
            ////            command-name-expr
            ////        redirections:
            ////            redirection
            ////            redirections   redirection
            ////        redirection:
            ////            2>&1
            ////            1>&2
            ////            file-redirection-operator   redirected-file-name
            ////        redirected-file-name:
            ////            command-argument
            ////            primary-expression
            #endregion
            #region B.2.3 Expressions
            ////        expression:
            ////            logical-expression
            ////        logical-expression:
            ////            bitwise-expression
            ////            logical-expression   -and   new-lines_opt   bitwise-expression
            ////            logical-expression   -or   new-lines_opt   bitwise-expression
            ////            logical-expression   -xor   new-lines_opt   bitwise-expression
            ////        bitwise-expression:
            ////            comparison-expression
            ////            bitwise-expression   -band   new-lines_opt   comparison-expression
            ////            bitwise-expression   -bor   new-lines_opt   comparison-expression
            ////            bitwise-expression   -bxor   new-lines_opt   comparison-expression
            ////        comparison-expression:
            ////            additive-expression
            ////            comparison-expression   comparison-operator   new-lines_opt   additive-expression
            ////        additive-expression:
            ////            multiplicative-expression
            ////            additive-expression   +   new-lines_opt   multiplicative-expression
            ////            additive-expression   dash   new-lines_opt   multiplicative-expression
            ////        multiplicative-expression:
            ////            format-expression
            ////            multiplicative-expression   *   new-lines_opt   format-expression
            ////            multiplicative-expression   /   new-lines_opt   format-expression
            ////            multiplicative-expression   %   new-lines_opt   format-expression
            ////        format-expression:
            ////            range-expression
            ////            format-expression   format-operator    new-lines_opt   range-expression
            ////        range-expression:
            ////            array-literal-expression
            ////            range-expression   ..   new-lines_opt   array-literal-expression
            ////        array-literal-expression:
            ////            unary-expression
            ////            unary-expression   ,    new-lines_opt   array-literal-expression
            ////        unary-expression:
            ////            primary-expression
            ////            expression-with-unary-operator
            ////        expression-with-unary-operator:
            ////            ,   new-lines_opt   unary-expression
            ////            -not   new-lines_opt   unary-expression
            ////            !   new-lines_opt   unary-expression
            ////            -bnot   new-lines_opt   unary-expression
            ////            +   new-lines_opt   unary-expression
            ////            dash   new-lines_opt   unary-expression
            ////            pre-increment-expression
            ////            pre-decrement-expression
            ////            cast-expression
            ////            -split   new-lines_opt   unary-expression
            ////            -join   new-lines_opt   unary-expression
            ////        pre-increment-expression:
            ////            ++   new-lines_opt   unary-expression
            ////        pre-decrement-expression:
            ////            dashdash   new-lines_opt   unary-expression
            ////        cast-expression:
            ////            type-literal   unary-expression
            ////        primary-expression:
            ////            value
            ////            member-access
            ////            element-access
            ////            invocation-expression
            ////            post-increment-expression
            ////            post-decrement-expression
            ////        value:
            ////            parenthesized-expression
            ////            sub-expression
            ////            array-expression
            ////            script-block-expression
            ////            hash-literal-expression
            ////            literal
            ////            type-literal
            ////            variable
            ////        parenthesized-expression:
            ////            (   new-lines_opt   pipeline   new-lines_opt   )
            ////        sub-expression:
            ////            $(   new-lines_opt   statement-list_opt   new-lines_opt   )
            ////        array-expression:
            ////            @(   new-lines_opt   statement-list_opt   new-lines_opt   )
            ////        script-block-expression:
            ////            {   new-lines_opt   script-block   new-lines_opt   }
            ////        hash-literal-expression:
            ////            @{   new-lines_opt   hash-literal-body_opt   new-lines_opt   }
            ////        hash-literal-body:
            ////            hash-entry
            ////            hash-literal-body   statement-terminators   hash-entry
            ////        hash-entry:
            ////            key-expression   =   new-lines_opt   statement
            ////        key-expression:
            ////            simple-name
            ////            unary-expression
            ////        post-increment-expression:
            ////            primary-expression   ++
            ////        post-decrement-expression:
            ////            primary-expression   dashdash
            ////        member-access: Note no whitespace is allowed between terms in these productions.
            ////            primary-expression   .   member-name
            ////        primary-expression   ::   member-name
            ////        element-access: Note no whitespace is allowed between primary-expression and [.
            ////            primary-expression   [  new-lines_opt   expression   new-lines_opt   ]
            ////        invocation-expression: Note no whitespace is allowed between terms in these productions.
            ////            primary-expression   .   member-name   argument-list
            ////        primary-expression   ::   member-name   argument-list
            ////        argument-list:
            ////            (   argument-expression-list_opt   new-lines_opt   )
            ////        argument-expression-list:
            ////            argument-expression
            ////            argument-expression   new-lines_opt   ,   argument-expression-list
            ////        argument-expression:
            ////            new-lines_opt   logical-argument-expression
            ////        logical-argument-expression:
            ////            bitwise-argument-expression
            ////            logical-argument-expression   -and   new-lines_opt   bitwise-argument-expression
            ////            logical-argument-expression   -or   new-lines_opt   bitwise-argument-expression
            ////            logical-argument-expression   -xor   new-lines_opt   bitwise-argument-expression
            ////        bitwise-argument-expression:
            ////            comparison-argument-expression
            ////            bitwise-argument-expression   -band   new-lines_opt   comparison-argument-expression
            ////            bitwise-argument-expression   -bor   new-lines_opt   comparison-argument-expression
            ////            bitwise-argument-expression   -bxor   new-lines_opt   comparison-argument-expression
            ////        comparison-argument-expression:
            ////            additive-argument-expression
            ////            comparison-argument-expression   comparison-operator
            ////                        new-lines_opt   additive-argument-expression
            ////        additive-argument-expression:
            ////            multiplicative-argument-expression
            ////            additive-argument-expression   +   new-lines_opt   multiplicative-argument-expression
            ////            additive-argument-expression   dash   new-lines_opt   multiplicative-argument-expression
            ////        multiplicative-argument-expression:
            ////            format-argument-expression
            ////            multiplicative-argument-expression   *   new-lines_opt   format-argument-expression
            ////            multiplicative-argument-expression   /   new-lines_opt   format-argument-expression
            ////            multiplicative-argument-expression   %   new-lines_opt   format-argument-expression
            ////        format-argument-expression:
            ////            range-argument-expression
            ////            format-argument-expression   format-operator   new-lines_opt   range-argument-expression
            ////        range-argument-expression:
            ////            unary-expression
            ////            range-expression   ..   new-lines_opt   unary-expression
            ////        member-name:
            ////            simple -name
            ////            string-literal
            ////            string-literal-with-subexpression
            ////            expression-with-unary-operator
            ////            value
            ////        string-literal-with-subexpression:
            ////            expandable-string-literal-with-subexpr
            ////            expandable-here-string-literal-with-subexpr
            ////        expandable-string-literal-with-subexpr:
            ////            expandable-string-with-subexpr-start   statement-list_opt   )
            ////                    expandable-string-with-subexpr-characters   expandable-string-with-subexpr-end
            ////            expandable-here-string-with-subexpr-start   statement-list_opt   )
            ////                    expandable-here-string-with-subexpr-characters
            ////                    expandable-here-string-with-subexpr-end
            ////        expandable-string-with-subexpr-characters:
            ////            expandable-string-with-subexpr-part
            ////            expandable-string-with-subexpr-characters   expandable-string-with-subexpr-part
            ////        expandable-string-with-subexpr-part:
            ////            sub-expression
            ////            expandable-string-part
            ////        expandable-here-string-with-subexpr-characters:
            ////            expandable-here-string-with-subexpr-part
            ////            expandable-here-string-with-subexpr-characters   expandable-here-string-with-subexpr-part
            ////        expandable-here-string-with-subexpr-part:
            ////            sub-expression
            ////            expandable-here-string-part
            ////        type-literal:
            ////            [    type-spec   ]
            ////        type-spec:
            ////            array-type-name    dimension_opt   ]
            ////            generic-type-name   generic-type-arguments   ]
            ////            type-name
            ////        dimension:
            ////            ,
            ////            dimension   ,
            ////        generic-type-arguments:
            ////            type-spec
            ////            generic-type-arguments   ,   type-spec
            #endregion
            #region B.2.4 Attributes
            ////        attribute-list:
            ////            attribute
            ////            attribute-list   new-lines_opt   attribute
            ////        attribute:
            ////            [   attribute-name   (   attribute-arguments   new-lines_opt   )  new-lines_opt   ]
            ////            type-literal
            ////        attribute-name:
            ////            type-spec
            ////        attribute-arguments:
            ////            attribute-argument
            ////            attribute-argument   new-lines_opt   ,   attribute-arguments
            ////        attribute-argument:
            ////            new-lines_opt   expression
            ////            new-lines_opt   simple-name   =   new-lines_opt   expression
            #endregion
            #endregion
        }
    }
}

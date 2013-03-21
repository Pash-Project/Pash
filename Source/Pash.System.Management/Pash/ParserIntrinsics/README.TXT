# Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

Parse tree definition.

Overview:
----------

In general, the lexical grammar is described as regular expressions in `PowerShellGrammar.Terminals.cs`, while the syntactic grammar is described as production rules in `PowerShellGrammar.cs`. 

Verbatim quotes from the language spec are preceded with 4 slashes, like this:

            ////        pipeline:
            ////            assignment_expression
            ////            expression   redirections_opt  pipeline_tail_opt
            ////            command   pipeline_tail_opt

Note that hyphens become underscores, and subscript (opt) becomes _opt.

When the lexical grammar is very complex, and the complexity is written as production rules. This makes consuming the parse tree far simpler. A good example is literals, presented as composed terminals in the spec:

            ////        literal:
            ////            integer_literal
            ////            real_literal
            ////            string_literal
            ////        string_literal:
            ////            expandable_string_literal
            ////            expandable_here_string_literal
            ////            verbatim_string_literal
            ////            verbatim_here_string_literal
            // etc.


Terminals:
----------

Consider:

            ////        decimal_digit:   one of
            ////            0   1   2   3   4   5   6   7   8   9
            public static readonly RegexBasedTerminal decimal_digit = null; // Initialized by reflection
            const string decimal_digit_pattern = "(?<decimal_digit>" + "[0123456789]" + ")";

Note that it's straightforward to read the comment and the pattern, and confirm that they are the same. 

The pattern is stored in a `const` so that they can be composed without concern for initialization order. Therefore, we can use the same order as the language spec. By contract, if I used `static` fields:

            static string pattern2 = pattern1 + "xx";
            static string pattern1 = pattern2 + "+";

This is clearly not OK, but the compiler allows it!

----

Occasionally, a terminal is defined as an exception of another rule, e.g.:

            ////        parameter_char:
            ////            Any Unicode character except
            ////                {	}	(	)	;	,	|	&	.	[
            ////                colon
            ////                whitespace
            ////                new_line_character

It's not possible to consume colon_pattern in this regex. That is, you can't write:

            const string colon_pattern = "(?<colon>" + colon_ + ")";
            const string parameter_char_pattern = "(?<parameter_char>" + @"[^\{\}\(\)\;\,\|\&\.\[" + colon_pattern + whitespace_ + new_line_character_ + "]" + ")";

Because the character class syntac ("[]") only allows single characters. To clarify, you can't write this regex:

        "[ab(c)]"

That's why I break the character list out in to yet another const string, like this:

            const string colon_ = @"\u003A";
            const string new_line_character_ = @"\u000D\u000A";

----

In some cases, when the the terminal description is somewhat complex, I break it in to small regular expressions:

            ////        variable:
            ////            $$
            ////            $?
            ////            $^
            ////            $   variable_scope_opt  variable_characters
            ////            @   variable_scope_opt   variable_characters
            ////            braced_variable
            public static readonly RegexBasedTerminal variable = null; // Initialized by reflection
            const string variable_pattern = "(?<variable>" +
                _dollar_dollar_pattern + "|" +
                _dollar_question_pattern + "|" +
                _dollar_hat_pattern + "|" +
                _ordinary_variable_pattern + "|" +
                _splatted_variable_pattern + "|" +
                braced_variable_pattern +
                ")";


            public static readonly RegexBasedTerminal _dollar_dollar = null; // Initialized by reflection
            const string _dollar_dollar_pattern = "(?<_dollar_dollar>" + @"\$\$" + ")";

            // etc.

----

If a rule is only partially implemented, leave a TODO comment as a marker:

            ////        decimal_integer_literal:
            ////            decimal_digits   numeric_type_suffix_opt   numeric_multiplier_opt
            // TODO: more
            public static readonly RegexBasedTerminal decimal_integer_literal = null; // Initialized by reflection
            const string decimal_integer_literal_pattern = "(?<decimal_integer_literal>" + decimal_digits_pattern + ")";

If it's 0% implemented, or 100% implemented, there's no need for the TODO.

NonTerminals
------------

NonTerminals can only be defined within the context of a Grammar object, so they can't be `const` or `static readonly`. 

Instead they are fields with Rules defined at runtime. Irony expects every `NonTerminal` to be initialized with a constructor that takes the rule name as a parameter, like this:

        NonTerminal Foo = new NonTerminal("Foo");

The Irony samples I've looked at declare these as locals in the grammar's ctor. That makes it difficult to inspect the parse tree, as you have to use strings (as far as I can tell):

       if (parseTreeNode.Name == "Foo") ...

This duplication, scattered throughout the code, is error-prone and tedious. To address that problem, I moved the NonTerminals to be fields, and initialized them with reflection. Now you can write:

        if (parseTreeNode.Term = grammar.Foo) ...

The downsides are:

- Go To Definition on Foo doesn't work well, as it takes you to the uninteresting field declaration, not the rule definition.

- you have to keep access to the grammar object. 

----

Some grammar rules are dificult to consume, e.g.:

            ////        statement:
            ////            if_statement
            ////            label_opt   labeled_statement
            ////            function_statement
            ////            flow_control_statement   statement_terminator
            ////            trap_statement
            ////            try_statement
            ////            data_statement
            ////            pipeline   statement_terminator

Note that some of these have 1 child, while others have 2. Dealing with that logic in the AST builder is a little messy. Ideally, all production rules would be written in one of these two forms:

        A:
            B
            C
            D

or

        E: 
           F + G + H

That is, use either + or |, but not both.

This lets the parser do the work of telling you which you're looking at. In some cases, I have modified the grammar to this pattern. In that case, the additional rules get special naming. For example

            ////        pipeline:
            ////            assignment_expression
            ////            expression   redirections_opt  pipeline_tail_opt
            ////            command   pipeline_tail_opt

becomes:

            //          pipeline:
            //            assignment_expression
            //            _pipeline_expression
            //            _pipeline_command

            //        _pipeline_expression:
            //            expression   redirections_opt  pipeline_tail_opt
            //
            //        _pipeline_command:
            //            command   pipeline_tail_opt
            pipeline.Rule = assignment_expression | _pipeline_expression | _pipeline_command;
            _pipeline_expression.Rule = expression + (pipeline_tail | Empty);
            _pipeline_command.Rule = command + (pipeline_tail | Empty);

It's OK to write an optional with "| Empty" - that doesn't create the same difficulties.


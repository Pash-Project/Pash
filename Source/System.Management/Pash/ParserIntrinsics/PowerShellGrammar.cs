// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using Irony.Parsing;
using System.Globalization;
using System.Text.RegularExpressions;
using Irony;
using System.Diagnostics;
using Extensions.Irony;
using System.Collections.Generic;

namespace Pash.ParserIntrinsics
{
    ////////
    // PowerShell Language Syntactic Grammar, as presented in the PowerShell Language Specification[1], Appendix B.2
    //
    // [1]: http://www.microsoft.com/en_us/download/details.aspx?id=9706
    ///////

    public partial class PowerShellGrammar : CaseInsensitiveGrammar
    {
        #region B.1 Lexical grammar

        #region B.1.1 Line terminators
        public readonly NonTerminal new_lines = null; // Initialized by reflection.
        public readonly Terminal skipped_new_line;
        #endregion

        #region B.1.8 Literals
        public readonly NonTerminal literal = null; // Initialized by reflection
        public readonly NonTerminal integer_literal = null; // Initialized by reflection
        public readonly NonTerminal string_literal = null; // Initialized by reflection
        #endregion

        #region B.1.11 Operators and punctuators

        public readonly NonTerminal comparison_operator = null; // Initialized by reflection.

        #endregion

        #endregion

        #region B.2 Syntactic grammar

        #region B.2.1 Basic concepts
        //public readonly NonTerminal script_file = null; // Initialized by reflection.
        //public readonly NonTerminal module_file = null; // Initialized by reflection.
        //public readonly NonTerminal interactive_input = null; // Initialized by reflection.
        //public readonly NonTerminal data_file = null; // Initialized by reflection.
        #endregion

        #region B.2.2 Statements
        public readonly NonTerminal script_block = null; // Initialized by reflection.
        public readonly NonTerminal param_block = null; // Initialized by reflection.
        public readonly NonTerminal param_block_opt = null; // Initialized by reflection.

        public readonly NonTerminal parameter_list = null; // Initialized by reflection.
        public readonly NonTerminal parameter_list_opt = null; // Initialized by reflection.
        public readonly NonTerminal _marked_parameter_list = null; // Initialized by reflection.
        public readonly NonTerminal script_parameter = null; // Initialized by reflection.
        public readonly NonTerminal script_parameter_default = null; // Initialized by reflection.
        public readonly NonTerminal script_parameter_default_opt = null; // Initialized by reflection.
        public readonly NonTerminal script_block_body = null; // Initialized by reflection.
        public readonly NonTerminal script_block_body_opt = null; // Initialized by reflection.
        public readonly NonTerminal named_block_list = null; // Initialized by reflection.
        public readonly NonTerminal named_block = null; // Initialized by reflection.
        public readonly NonTerminal block_name = null; // Initialized by reflection.
        public readonly NonTerminal statement_block = null; // Initialized by reflection.
        public readonly NonTerminal _statement_block_empty = null; // Initialized by reflection.
        public readonly NonTerminal _statement_block_full = null; // Initialized by reflection.
        public readonly NonTerminal statement_list = null; // Initialized by reflection.
        public readonly NonTerminal _statement_list_rest = null; // Initialized by reflection.
        public readonly NonTerminal statement_list_opt = null; // Initialized by reflection.
        public readonly NonTerminal statement = null; // Initialized by reflection.
        public readonly NonTerminal _terminated_statement = null; // Initialized by reflection.
        public readonly NonTerminal _unterminated_statement = null; // Initialized by reflection.
        public readonly NonTerminal _statement_labeled_statement = null; // Initialized by reflection.
        public readonly NonTerminal statement_terminator = null; // Initialized by reflection.
        public readonly NonTerminal statement_terminators = null; // Initialized by reflection.
        public readonly NonTerminal statement_terminators_opt = null; // Initialized by reflection.
        public readonly NonTerminal if_statement = null; // Initialized by reflection.
        public readonly NonTerminal _if_statement_clause = null; // Initialized by reflection.
        public readonly NonTerminal _if_statement_condition = null; // Initialized by reflection.
        public readonly NonTerminal elseif_clauses = null; // Initialized by reflection.
        public readonly NonTerminal elseif_clause = null; // Initialized by reflection.
        public readonly NonTerminal else_clause = null; // Initialized by reflection.
        public readonly NonTerminal else_clause_opt = null; // Initialized by reflection.
        public readonly NonTerminal labeled_statement = null; // Initialized by reflection.
        public readonly NonTerminal switch_statement = null; // Initialized by reflection.
        public readonly NonTerminal switch_parameters = null; // Initialized by reflection.
        public readonly NonTerminal switch_parameters_opt = null; // Initialized by reflection.
        public readonly NonTerminal switch_parameter = null; // Initialized by reflection.
        public readonly NonTerminal switch_condition = null; // Initialized by reflection.
        public readonly NonTerminal _switch_condition_pipeline = null; // Initialized by reflection.
        public readonly NonTerminal _switch_condition_file = null; // Initialized by reflection.
        public readonly NonTerminal switch_filename = null; // Initialized by reflection.
        public readonly NonTerminal switch_body = null; // Initialized by reflection.
        public readonly NonTerminal switch_clauses = null; // Initialized by reflection.
        public readonly NonTerminal switch_clause = null; // Initialized by reflection.
        public readonly NonTerminal switch_clause_condition = null; // Initialized by reflection.
        public readonly NonTerminal foreach_statement = null; // Initialized by reflection.
        public readonly NonTerminal for_statement = null; // Initialized by reflection.
        public readonly NonTerminal _for_statement_internals = null; // Initialized by reflection.
        public readonly NonTerminal for_initializer = null; // Initialized by reflection.
        public readonly NonTerminal for_initializer_opt = null; // Initialized by reflection.
        public readonly NonTerminal for_condition = null; // Initialized by reflection.
        public readonly NonTerminal for_condition_opt = null; // Initialized by reflection.
        public readonly NonTerminal for_iterator = null; // Initialized by reflection.
        public readonly NonTerminal for_iterator_opt = null; // Initialized by reflection.
        public readonly NonTerminal while_statement = null; // Initialized by reflection.
        public readonly NonTerminal do_statement = null; // Initialized by reflection.
        public readonly NonTerminal _do_statement_while = null; // Initialized by reflection.
        public readonly NonTerminal _do_statement_until = null; // Initialized by reflection.
        public readonly NonTerminal while_condition = null; // Initialized by reflection.
        public readonly NonTerminal function_statement = null; // Initialized by reflection.
        public readonly NonTerminal _function_or_filter_keyword = null; // Initialized by reflection.
        public readonly NonTerminal function_name = null; // Initialized by reflection.
        public readonly NonTerminal function_parameter_declaration = null; // Initialized by reflection.
        public readonly NonTerminal function_parameter_declaration_opt = null; // Initialized by reflection.
        public readonly NonTerminal flow_control_statement = null; // Initialized by reflection.
        public readonly NonTerminal _flow_control_statement_break = null; // Initialized by reflection.
        public readonly NonTerminal _flow_control_statement_continue = null; // Initialized by reflection.
        public readonly NonTerminal _flow_control_statement_throw = null; // Initialized by reflection.
        public readonly NonTerminal _flow_control_statement_return = null; // Initialized by reflection.
        public readonly NonTerminal _flow_control_statement_exit = null; // Initialized by reflection.
        public readonly NonTerminal label_expression = null; // Initialized by reflection.
        public readonly NonTerminal label_expression_opt = null; // Initialized by reflection.
        public readonly NonTerminal trap_statement = null; // Initialized by reflection.
        public readonly NonTerminal try_statement = null; // Initialized by reflection.
        public readonly NonTerminal _try_statement_catch = null; // Initialized by reflection.
        public readonly NonTerminal _try_statement_finally = null; // Initialized by reflection.
        public readonly NonTerminal _try_statement_catch_finally = null; // Initialized by reflection.
        public readonly NonTerminal catch_clauses = null; // Initialized by reflection.
        public readonly NonTerminal catch_clause = null; // Initialized by reflection.
        public readonly NonTerminal catch_type_list = null; // Initialized by reflection.
        public readonly NonTerminal catch_type_list_opt = null; // Initialized by reflection.
        public readonly NonTerminal finally_clause = null; // Initialized by reflection.
        public readonly NonTerminal data_statement = null; // Initialized by reflection.
        public readonly NonTerminal data_name = null; // Initialized by reflection.
        public readonly NonTerminal data_commands_allowed = null; // Initialized by reflection.
        public readonly NonTerminal data_commands_allowed_opt = null; // Initialized by reflection.
        public readonly NonTerminal data_commands_list = null; // Initialized by reflection.
        public readonly NonTerminal data_command = null; // Initialized by reflection.
        public readonly NonTerminal pipeline = null; // Initialized by reflection.
        public readonly NonTerminal pipeline_opt = null; // Initialized by reflection.
        public readonly NonTerminal _pipeline_expression = null; // Initialized by reflection.
        public readonly NonTerminal _pipeline_command = null; // Initialized by reflection.
        public readonly NonTerminal assignment_expression = null; // Initialized by reflection.
        public readonly NonTerminal pipeline_tail = null; // Initialized by reflection.
        public readonly NonTerminal pipeline_tails = null; // Initialized by reflection.
        public readonly NonTerminal command = null; // Initialized by reflection.
        public readonly NonTerminal _command_simple = null; // Initialized by reflection.
        public readonly NonTerminal _command_invocation = null; // Initialized by reflection.
        public readonly NonTerminal command_invocation_operator = null; // Initialized by reflection.
        public readonly NonTerminal command_module = null; // Initialized by reflection.
        public readonly NonTerminal command_name = null; // Initialized by reflection.
        public readonly NonTerminal generic_token_with_subexpr = null; // Initialized by reflection.
        public readonly NonTerminal command_name_expr = null; // Initialized by reflection.
        public readonly NonTerminal command_elements = null; // Initialized by reflection.
        public readonly NonTerminal command_elements_opt = null; // Initialized by reflection.
        public readonly NonTerminal command_element = null; // Initialized by reflection.
        public readonly NonTerminal command_argument = null; // Initialized by reflection.
        public readonly NonTerminal redirections = null; // Initialized by reflection.
        public readonly NonTerminal redirections_opt = null; // Initialized by reflection.
        public readonly NonTerminal redirection = null; // Initialized by reflection.
        public readonly NonTerminal _redirection_error_to_output = null; // Initialized by reflection.
        public readonly NonTerminal _redirection_reserved = null; // Initialized by reflection.
        public readonly NonTerminal redirected_file_name = null; // Initialized by reflection.
        #endregion

        #region B.2.3 Expressions
        public readonly NonTerminal expression = null; // Initialized by reflection.
        public readonly NonTerminal logical_expression = null; // Initialized by reflection.
        public readonly NonTerminal _logical_expression_operator = null; // Initialized by reflection.
        public readonly NonTerminal bitwise_expression = null; // Initialized by reflection.
        public readonly NonTerminal _bitwise_expression_operator = null; // Initialized by reflection.
        public readonly NonTerminal comparison_expression = null; // Initialized by reflection.
        public readonly NonTerminal additive_expression = null; // Initialized by reflection.
        public readonly NonTerminal _additive_expression_operator = null; // Initialized by reflection.
        public readonly NonTerminal multiplicative_expression = null; // Initialized by reflection.
        public readonly NonTerminal _multiplicative_expression_operator = null; // Initialized by reflection.
        public readonly NonTerminal format_expression = null; // Initialized by reflection.
        public readonly NonTerminal range_expression = null; // Initialized by reflection.
        public readonly NonTerminal array_literal_expression = null; // Initialized by reflection.
        public readonly NonTerminal unary_expression = null; // Initialized by reflection.
        public readonly NonTerminal expression_with_unary_operator = null; // Initialized by reflection.
/* TODO: all subexpressions of expression_with_unary_operator are currently merged into one for performance
        public readonly NonTerminal _unary_array_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_not_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_bang_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_bnot_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_plus_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_dash_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_split_expression = null; // Initialized by reflection.
        public readonly NonTerminal _unary_join_expression = null; // Initialized by reflection.
        public readonly NonTerminal pre_increment_expression = null; // Initialized by reflection.
        public readonly NonTerminal pre_decrement_expression = null; // Initialized by reflection.
// They are all replaced by the following:
*/
        public readonly NonTerminal _unary_operator_expression = null;
        public readonly NonTerminal _joined_unary_operator_expression = null;
        public readonly NonTerminal cast_expression = null; // Initialized by reflection.
        public readonly NonTerminal primary_expression = null; // Initialized by reflection.
        public readonly NonTerminal value = null; // Initialized by reflection.
        public readonly NonTerminal parenthesized_expression = null; // Initialized by reflection.
        public readonly NonTerminal sub_expression = null; // Initialized by reflection.
        public readonly NonTerminal array_expression = null; // Initialized by reflection.
        public readonly NonTerminal script_block_expression = null; // Initialized by reflection.
        public readonly NonTerminal hash_literal_expression = null; // Initialized by reflection.
        public readonly NonTerminal hash_literal_body = null; // Initialized by reflection.
        public readonly NonTerminal hash_literal_body_opt = null; // Initialized by reflection.
        public readonly NonTerminal hash_entry = null; // Initialized by reflection.
        public readonly NonTerminal key_expression = null; // Initialized by reflection.
        public readonly NonTerminal post_increment_expression = null; // Initialized by reflection.
        public readonly NonTerminal post_decrement_expression = null; // Initialized by reflection.
        public readonly NonTerminal _member_access_or_invocation_expression = null; // Initialized by reflection.
        public readonly NonTerminal _member_access_or_invocation_expression_operator = null; // Initialized by reflection.
        public readonly NonTerminal element_access = null; // Initialized by reflection.
        public readonly NonTerminal argument_list = null; // Initialized by reflection.
        public readonly NonTerminal argument_list_opt = null; // Initialized by reflection.
        public readonly NonTerminal argument_expression_list = null; // Initialized by reflection.
        public readonly NonTerminal argument_expression_list_opt = null; // Initialized by reflection.
        public readonly NonTerminal argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal logical_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal bitwise_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal comparison_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal additive_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal multiplicative_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal format_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal range_argument_expression = null; // Initialized by reflection.
        public readonly NonTerminal member_name = null; // Initialized by reflection.
        public readonly NonTerminal string_literal_with_subexpression = null; // Initialized by reflection.
        public readonly NonTerminal expandable_string_literal_with_subexpr = null; // Initialized by reflection.
        public readonly NonTerminal expandable_string_with_subexpr_characters = null; // Initialized by reflection.
        public readonly NonTerminal expandable_string_with_subexpr_characters_opt = null; // Initialized by reflection.
        public readonly NonTerminal expandable_string_with_subexpr_part = null; // Initialized by reflection.
        public readonly NonTerminal expandable_here_string_with_subexpr_characters = null; // Initialized by reflection.
        public readonly NonTerminal expandable_here_string_with_subexpr_part = null; // Initialized by reflection.
        public readonly NonTerminal type_literal = null; // Initialized by reflection.
        public readonly NonTerminal type_literal_opt = null; // Initialized by reflection.
        public readonly NonTerminal type_spec = null; // Initialized by reflection.
        public readonly NonTerminal _type_spec_array = null; // Initialized by reflection.
        public readonly NonTerminal _type_spec_array_separators = null; // Initialized by reflection.
        public readonly NonTerminal _type_spec_generic = null; // Initialized by reflection.
        public readonly NonTerminal dimension = null; // Initialized by reflection.
        public readonly NonTerminal generic_type_arguments = null; // Initialized by reflection.
        #endregion

        #region B.2.4 Attributes
        public readonly NonTerminal attribute_list = null; // Initialized by reflection.
        public readonly NonTerminal attribute = null; // Initialized by reflection.
        public readonly NonTerminal attribute_name = null; // Initialized by reflection.
        public readonly NonTerminal attribute_arguments = null; // Initialized by reflection.
        public readonly NonTerminal attribute_argument = null; // Initialized by reflection.
        #endregion

        #endregion

        #region Workaround related

        public readonly ImpliedSymbolTerminal _begin_paramlist_marker = new ImpliedSymbolTerminal("beginParamList");
        public readonly ImpliedSymbolTerminal _end_paramlist_marker = new ImpliedSymbolTerminal("endParamList");

        #endregion

        private Tuple<KeyTerm, Regex>[] _terms_after_newline;

        public PowerShellGrammar()
        {
            InitializeTerminalFields();
            InitializeNonTerminalFields();
            InitializeComments();

            // delegate to a new method, so we don't accidentally overwrite a readonly field.
            BuildProductionRules();

            // there are special terms that can follow after skippable newlines. These terms are the
            // param block, elseif and else blocks. To make sure they aren't interpreted as statements,
            // we need to extra check when a newline occurs if the newline should be skipped
            _terms_after_newline = (
                from t in new KeyTerm[] { @param, @else, @elseif }
                select new Tuple<KeyTerm, Regex>(t, new Regex(new_line_character.Pattern + "*" + t.Text))
            ).ToArray();

            // Skip newlines within statements, e.g.:
            //     if ($true)       # not the end of the statement - keep parsing!
            //     {
            //         "hi"
            //     }
            this.skipped_new_line = new SkippedTerminal(this.new_line_character);
            new_line_character.ValidateToken += delegate(object sender, ValidateTokenEventArgs validateTokenEventArgs)
            {
                if (!IsNewlineExpected(validateTokenEventArgs))
                {
                    validateTokenEventArgs.ReplaceToken(this.skipped_new_line);
                }
            };

            NonGrammarTerminals.Add(new BacktickLineContinuationTerminal());

            Root = this.script_block;
        }

        bool IsNewlineExpected(ValidateTokenEventArgs validateTokenEventArgs)
        {
            // Here we check manually if a newline is used as a statement terminator
            // or if it can be skipped (e.g. after `if(exp)`
            // First we have some special cases:

            // detect param, elseif, and else keywords to make sure they aren't misinterpreted as a command
            foreach (var matchTuple in _terms_after_newline)
            {
                if (validateTokenEventArgs.Context.CurrentParserState.ExpectedTerminals.Contains(matchTuple.Item1) &&
                    matchTuple.Item2.IsMatch(validateTokenEventArgs.Context.Source.Text,
                                             validateTokenEventArgs.Context.Source.Position))
                {
                    return false;
                }
            }

            // Now the general case: When the parser has any matching rule witht the newline, it's expected.
            // Otherwise it's optional and we skip it.
            // Remember that `validateTokenEventArgs.Context.CurrentParserState` points in to the finite state machine
            // that Irony generates. `Actions` represents the valid transitions to new states. If this statement is
            // `true`, it means the grammar has a plan for what to do with that `new_line_character`.
            return validateTokenEventArgs.Context.CurrentParserState.Actions.ContainsKey(this.new_line_character);
        }

        public override void OnLanguageDataConstructed(LanguageData language)
        {
            if (language.ErrorLevel != GrammarErrorLevel.NoError) throw new Exception(language.ErrorLevel.ToString());
        }

        public void BuildProductionRules()
        {

            #region B.1 Lexical grammar

            // this was presented as part of the lexical grammar, but I'd rather see this as production rules than 
            // as regex patterns.

            #region B.1.1 Line terminators

            ////        new_lines:
            ////            new_line_character
            ////            new_lines   new_line_character
            new_lines.Rule =
                MakePlusRule(new_lines, new_line_character);

            #endregion

            #region B.1.8 Literals
            ////        literal:
            ////            integer_literal
            ////            real_literal
            ////            string_literal
            literal.Rule =
                integer_literal
                |
                real_literal
                |
                string_literal
                |
                string_literal_with_subexpression
                ;

            ////        integer_literal:
            ////            decimal_integer_literal
            ////            hexadecimal_integer_literal
            integer_literal.Rule =
                decimal_integer_literal
                |
                hexadecimal_integer_literal
                ;

            ////        real-literal:
            ////            decimal-digits   .   decimal-digits   exponent-partopt   decimal-type-suffixopt   numeric-multiplieropt
            ////            .   decimal-digits   exponent-partopt   decimal-type-suffixopt   numeric-multiplieropt
            ////            decimal-digits   exponent-part  decimal-type-suffixopt   numeric-multiplieropt
            ////
            ////            exponent-part:
            ////                "e"   sign_opt   decimal-digits
            ////
            ////            sign:   one of
            ////                "+"
            ////                dash
            ////
            ////            decimal-type-suffix:
            ////                "d"

            ////        string_literal:
            ////            expandable_string_literal
            ////            expandable_here_string_literal
            ////            verbatim_string_literal
            ////            verbatim_here_string_literal
            string_literal.Rule =
                expandable_string_literal
                |
                expandable_here_string_literal
                |
                verbatim_string_literal
                |
                verbatim_here_string_literal
                ;
            #endregion

            #region B.1.11 Operators and punctuators

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

            // tiny comments to keep the autoformatter from destroying the columns.
            comparison_operator.Rule =
                       _comparison_operator_as					/**/ | _comparison_operator_ccontains				/**/ | _comparison_operator_ceq
                /**/ | _comparison_operator_cge					/**/ | _comparison_operator_cgt						/**/ | _comparison_operator_cle
                /**/ | _comparison_operator_clike				/**/ | _comparison_operator_clt						/**/ | _comparison_operator_cmatch
                /**/ | _comparison_operator_cne					/**/ | _comparison_operator_cnotcontains			/**/ | _comparison_operator_cnotlike
                /**/ | _comparison_operator_cnotmatch			/**/ | _comparison_operator_contains				/**/ | _comparison_operator_creplace
                /**/ | _comparison_operator_csplit				/**/ | _comparison_operator_eq						/**/ | _comparison_operator_ge
                /**/ | _comparison_operator_gt					/**/ | _comparison_operator_icontains				/**/ | _comparison_operator_ieq
                /**/ | _comparison_operator_ige					/**/ | _comparison_operator_igt						/**/ | _comparison_operator_ile
                /**/ | _comparison_operator_ilike				/**/ | _comparison_operator_ilt						/**/ | _comparison_operator_imatch
                /**/ | _comparison_operator_ine					/**/ | _comparison_operator_inotcontains			/**/ | _comparison_operator_inotlike
                /**/ | _comparison_operator_inotmatch			/**/ | _comparison_operator_ireplace				/**/ | _comparison_operator_is
                /**/ | _comparison_operator_isnot				/**/ | _comparison_operator_isplit					/**/ | _comparison_operator_join
                /**/ | _comparison_operator_le					/**/ | _comparison_operator_like					/**/ | _comparison_operator_lt
                /**/ | _comparison_operator_match				/**/ | _comparison_operator_ne						/**/ | _comparison_operator_notcontains
                /**/ | _comparison_operator_notlike				/**/ | _comparison_operator_notmatch				/**/ | _comparison_operator_replace
                /**/ | _comparison_operator_split
                ;

            #endregion

            #endregion

            #region B.2 Syntactic grammar

            #region B.2.1 Basic concepts
            ////        script_file:
            ////            script_block
            ////        module_file:
            ////            script_block
            ////        interactive_input:
            ////            script_block
            ////        data_file:
            ////            statement_list
            #endregion

            #region B.2.2 Statements
            ////        script_block:
            ////            param_block_opt   statement_terminators_opt    script_block_body_opt
            // NOTE: the script_block_body_opt includes a statement_list that can only consist of statement_terminators
            //       Therefore we need to leave out the statement_terminators_opt here or we get shift/reduce errors!
            script_block.Rule =
                param_block_opt + script_block_body_opt;

            ////        param_block:
            ////            new_lines_opt   attribute_list_opt   new_lines_opt   param   new_lines_opt
            ////                    (   parameter_list_opt   new_lines_opt   )
            // NOTE: use _marked_parameter_list here, a parameter_list_opt with implicit marker symbols
            param_block.Rule =
                /*  TODO: https://github.com/Pash-Project/Pash/issues/11 attribute_list_opt +  */ @param
                + _marked_parameter_list;

            // ISSUE: https://github.com/Pash-Project/Pash/issues/367
            // multiple parameters with default values were not possible, because they were mistaken for a 
            // literal array. They aren't allowed anymore without brakets inside a parameter list. To parse this
            // in a context sensitive manner, implicit marker symbols and a custom action in array_literal_expression
            // are used
            _marked_parameter_list.Rule = "(" + _begin_paramlist_marker + parameter_list_opt + ")" + _end_paramlist_marker;

            ////        parameter_list:
            ////            script_parameter
            ////            parameter_list   new_lines_opt   ,   script_parameter
            parameter_list.Rule = 
                MakePlusRule(parameter_list, ToTerm(","), script_parameter)
                ;

            ////        script_parameter:
            ////            new_lines_opt   attribute_list_opt   new_lines_opt   variable   script_parameter_default_opt
            script_parameter.Rule =
                /* TODO: https://github.com/Pash-Project/Pash/issues/11 attribute_list_opt +  */ variable + script_parameter_default_opt;

            ////        script_parameter_default:
            ////            new_lines_opt   =   new_lines_opt   expression
            script_parameter_default.Rule =
                 "=" + expression;

            ////        script_block_body:
            ////            named_block_list
            ////            statement_list
            script_block_body.Rule =
                named_block_list
                |
                statement_list
                ;

            ////        named_block_list:
            ////            named_block
            ////            named_block_list   named_block
            named_block_list.Rule =
                MakePlusRule(named_block_list, named_block);

            ////        named_block:
            ////            block_name   statement_block   statement_terminators_opt
            named_block.Rule =
                block_name + statement_block + statement_terminators_opt;

            ////        block_name:  one of
            ////            dynamicparam   begin   process   end
            block_name.Rule =
                @dynamicparam | @begin | @process | @end;

            ////        statement_block:
            ////            new_lines_opt   {   statement_list_opt   new_lines_opt   }
            statement_block.Rule =
                "{" + statement_list_opt + "}";

            ////        statement_list:
            ////            statement
            ////            statement_list   statement

            // We split this rule in _unterminated_statements that require a statement_terminator behind them and
            // _terminated_statements which are terminated by itself. Note that the last _unterminated_statement
            // doesn't require a statement_terminator so we add one optional.
            // Also, there can be a number of statement_terminators before and after the list.

            statement_list.Rule =
                _statement_list_rest // the last statement
                |
                _terminated_statement + statement_list
                |
                _unterminated_statement + statement_terminator + statement_list
                |
                statement_terminator + statement_list ;

            _statement_list_rest.Rule =
                _unterminated_statement
                |
                statement_terminator
                |
                _terminated_statement
                |
                // is necessary to allow an unterminated_statement with one terminator. other stuff works with the list
                _unterminated_statement + statement_terminator
                ;


            ////        statement:
            ////            if_statement
            ////            label_opt   labeled_statement
            ////            function_statement
            ////            flow_control_statement   statement_terminator
            ////            trap_statement
            ////            try_statement
            ////            data_statement
            ////            pipeline   statement_terminator

            // The spec has a bug concerning the definition of the statement_terminator:
            // Easy example: `if ($true) { }` shouldn't require a semicolon or newline, a pipeline needs one
            // Also: pipeline includes a recursive rule on statement, which would require multiple terminators
            // Therefore we split the stamentens in terminated statements and unterminated once that require
            // terminators as separators in the statement_list. See https://github.com/Pash-Project/Pash/issues/7
            statement.Rule =
                _unterminated_statement
                |
                _terminated_statement
                ;

            // The spec doesn't define `label`. I'm using `simple_name` for that purpose.
            _terminated_statement.Rule =
                if_statement
                |
                _statement_labeled_statement
                |
                function_statement
                |
                trap_statement
                |
                try_statement
                |
                data_statement
                ;

            _unterminated_statement.Rule =
                flow_control_statement
                |
                pipeline
                ;

            _statement_labeled_statement.Rule =
                label + labeled_statement
                |
                labeled_statement
                ;

            ////        statement_terminator:
            ////            ;
            ////            new_line_character
            var semicolon = ToTerm(";");
            semicolon.SetFlag(TermFlags.IsTransient);

            statement_terminator.Rule =
                semicolon
                |
                new_line_character
                ;
            MarkTransient(statement_terminator);

            ////        statement_terminators:
            ////            statement_terminator
            ////            statement_terminators   statement_terminator
            statement_terminators.Rule =
                MakePlusRule(statement_terminators, statement_terminator);

            ////        if_statement:
            ////            if   new_lines_opt   (   new_lines_opt   pipeline   new_lines_opt   )   statement_block elseif_clauses_opt   else_clause_opt
            if_statement.Rule =
                @if + _if_statement_clause + elseif_clauses + else_clause_opt
                ;

            ////        elseif_clause:
            ////            new_lines_opt   elseif   new_lines_opt   (   new_lines_opt   pipeline   new_lines_opt   )   statement_block
            elseif_clause.Rule =
                @elseif + _if_statement_clause
                ;

            ////        elseif_clauses:
            ////            elseif_clause
            ////            elseif_clauses   elseif_clause
            elseif_clauses.Rule =
                MakeStarRule(elseif_clauses, elseif_clause);

            _if_statement_clause.Rule = _if_statement_condition + statement_block;
            _if_statement_condition.Rule = "(" + pipeline + ")";

            ////        else_clause:
            ////            new_lines_opt   else   statement_block
            else_clause.Rule =
                 @else + statement_block;

            ////        labeled_statement:
            ////            switch_statement
            ////            foreach_statement
            ////            for_statement
            ////            while_statement
            ////            do_statement
            labeled_statement.Rule =
                switch_statement
                |
                foreach_statement
                |
                for_statement
                |
                while_statement
                |
                do_statement
                ;

            ////        switch_statement:
            ////            switch   new_lines_opt   switch_parameters_opt   switch_condition   switch_body
            switch_statement.Rule =
                @switch + switch_parameters_opt + switch_condition + switch_body;

            ////        switch_parameters:
            ////            switch_parameter
            ////            switch_parameters   switch_parameter
            switch_parameters.Rule =
                MakePlusRule(switch_parameters, switch_parameter);

            ////        switch_parameter:
            ////            _regex
            ////            _wildcard
            ////            _exact
            ////            _casesensitive
            switch_parameter.Rule =
                ToTerm("-regex")
                |
                "-wildcard"
                |
                "-exact"
                |
                "-casesensitive"
                ;

            ////        switch_condition:
            ////            (   new_lines_opt   pipeline   new_lines_opt   )
            ////            _file   new_lines_opt   switch_filename
            switch_condition.Rule = _switch_condition_pipeline | _switch_condition_file;
            _switch_condition_pipeline.Rule = "(" + pipeline + ")";
            _switch_condition_file.Rule = "-file" + switch_filename;

            ////        switch_filename:
            ////            command_argument
            ////            primary_expression
            switch_filename.Rule =
                command_argument
                //|
                //primary_expression
                ;

            ////        switch_body:
            ////            new_lines_opt   {   new_lines_opt   switch_clauses   }
            switch_body.Rule =
                 "{" + switch_clauses + "}";

            ////        switch_clauses:
            ////            switch_clause
            ////            switch_clauses   switch_clause
            switch_clauses.Rule =
                MakePlusRule(switch_clauses, switch_clause);

            ////        switch_clause:
            ////            switch_clause_condition   statement_block   statement_terimators_opt [sic]
            switch_clause.Rule =
                switch_clause_condition + statement_block + statement_terminators_opt;

            ////        switch_clause_condition:
            ////            command_argument
            ////            primary_expression
            switch_clause_condition.Rule =
                command_argument
                //|
                //primary_expression
                ;

            ////        foreach_statement:
            ////            foreach   new_lines_opt   (   new_lines_opt   variable   new_lines_opt   in   new_lines_opt   pipeline
            ////                    new_lines_opt   )   statement_block
            foreach_statement.Rule =
                @foreach + "(" + variable + @in + pipeline +
                     ")" + statement_block;

            ////        for_statement:
            ////            for   new_lines_opt   (
            ////                    new_lines_opt   for_initializer_opt   statement_terminator
            ////                    new_lines_opt   for_condition_opt   statement_terminator
            ////                    new_lines_opt   for_iterator_opt
            ////                    new_lines_opt   )   statement_block
            ////            for   new_lines_opt   (
            ////                    new_lines_opt   for_initializer_opt   statement_terminator
            ////                    new_lines_opt   for_condition_opt
            ////                    new_lines_opt   )   statement_block
            ////            for   new_lines_opt   (
            ////                    new_lines_opt   for_initializer_opt
            ////                    new_lines_opt   )   statement_block
            _for_statement_internals.Rule =
                MakePlusRule(_for_statement_internals, statement_terminator, pipeline_opt);
            for_statement.Rule =
                @for + "(" + _for_statement_internals + ")" + statement_block;

            ////        for_initializer:
            ////            pipeline
            for_initializer.Rule =
                pipeline;

            ////        for_condition:
            ////            pipeline
            for_condition.Rule =
                pipeline;

            ////        for_iterator:
            ////            pipeline
            for_iterator.Rule =
                pipeline;

            ////        while_statement:
            ////            while   new_lines_opt   (   new_lines_opt   while_condition   new_lines_opt   )   statement_block
            while_statement.Rule =
                @while + "(" + while_condition + ")" + statement_block;

            ////        do_statement:
            ////            do   statement_block  new_lines_opt   while   new_lines_opt   (   while_condition   new_lines_opt   )
            ////            do   statement_block   new_lines_opt   until   new_lines_opt   (   while_condition   new_lines_opt   )
            do_statement.Rule = _do_statement_while | _do_statement_until;
            _do_statement_while.Rule = @do + statement_block + @while + "(" + while_condition + ")";
            _do_statement_until.Rule = @do + statement_block + @until + "(" + while_condition + ")";

            ////        while_condition:
            ////            new_lines_opt   pipeline
            while_condition.Rule =
                 pipeline;

            ////        function_statement:
            ////            function   new_lines_opt   function_name   function_parameter_declaration_opt   {   script_block   }
            ////            filter   new_lines_opt   function_name   function_parameter_declaration_opt   {   script_block   }
            function_statement.Rule =
                 _function_or_filter_keyword + function_name + function_parameter_declaration_opt + "{" + script_block + "}";
            _function_or_filter_keyword.Rule = @function | @filter;

            ////        function_name:
            ////            command_argument
            function_name.Rule =
                generic_token
                //command_argument // I don't know how this can be anything other than a generic_token
                ;

            ////        function_parameter_declaration:
            ////            new_lines_opt   (   parameter_list   new_lines_opt   )
            // ISSUE: https://github.com/Pash-Project/Pash/issues/203
            // parameter_list was changed to _marked_parameter_list here, which includes parameter_list_opt
            // it is not in accordance with the published grammar, but otherwise
            // an empty parameter list wouldn't be allowed.
            function_parameter_declaration.Rule =
                 _marked_parameter_list;

            ////        flow_control_statement:
            ////            break   label_expression_opt
            ////            continue   label_expression_opt
            ////            throw    pipeline_opt
            ////            return   pipeline_opt
            ////            exit   pipeline_opt
            flow_control_statement.Rule = _flow_control_statement_break | _flow_control_statement_continue | _flow_control_statement_throw | _flow_control_statement_return | _flow_control_statement_exit;
            _flow_control_statement_break.Rule = @break + label_expression_opt;
            _flow_control_statement_continue.Rule = @continue + label_expression_opt;
            _flow_control_statement_throw.Rule = @throw + pipeline_opt;
            _flow_control_statement_return.Rule = @return + pipeline_opt;
            _flow_control_statement_exit.Rule = @exit + pipeline_opt;

            ////        label_expression:
            ////            simple_name
            ////            unary_expression
            label_expression.Rule =
                simple_name
                |
                unary_expression
                ;

            ////        trap_statement:
            ////            trap  new_lines_opt   type_literal_opt   new_lines_opt   statement_block
            trap_statement.Rule =
                @trap + type_literal_opt + statement_block;

            ////        try_statement:
            ////            try   statement_block   catch_clauses
            ////            try   statement_block   finally_clause
            ////            try   statement_block   catch_clauses   finally_clause
            try_statement.Rule =
                _try_statement_catch | _try_statement_finally | _try_statement_catch_finally;
            _try_statement_catch.Rule =
                @try + statement_block + catch_clauses;
            _try_statement_finally.Rule =
                @try + statement_block + finally_clause;
            _try_statement_catch_finally.Rule =
                @try + statement_block + catch_clauses + finally_clause;

            ////        catch_clauses:
            ////            catch_clause
            ////            catch_clauses   catch_clause
            catch_clauses.Rule =
                MakePlusRule(catch_clauses, catch_clause);

            ////        catch_clause:
            ////            new_lines_opt   catch   catch_type_list_opt   statement_block
            catch_clause.Rule =
                 @catch + catch_type_list_opt + statement_block;

            ////        catch_type_list:
            ////            new_lines_opt   type_literal
            ////            catch_type_list   new_lines_opt   ,   new_lines_opt   type_literal
            catch_type_list.Rule =
                MakePlusRule(catch_type_list, ToTerm(","), type_literal);

            ////        finally_clause:
            ////            new_lines_opt   finally   statement_block
            finally_clause.Rule =
                 @finally + statement_block;

            ////        data_statement:
            ////            data    new_lines_opt   data_name   data_commands_allowed_opt   statement_block
            data_statement.Rule =
                @data + data_name + data_commands_allowed_opt + statement_block;

            ////        data_name:
            ////            simple_name
            data_name.Rule =
                simple_name;

            ////        data_commands_allowed:
            ////            new_lines_opt   _supportedcommand   data_commands_list
            data_commands_allowed.Rule =
                 "-supportedcommand" + data_commands_list;

            ////        data_commands_list:
            ////            new_lines_opt   data_command
            ////            data_commands_list   ,   new_lines_opt   data_command
            data_commands_list.Rule =
                MakePlusRule(data_commands_list, ToTerm(","), (data_command));

            ////        data_command:
            ////            command_name_expr
            data_command.Rule =
                command_name_expr;

            ////        pipeline:
            ////            assignment_expression
            ////            expression   redirections_opt  pipeline_tail_opt
            ////            command   pipeline_tail_opt
            ////        pipeline_tail:
            ////            |   new_lines_opt   command
            ////            |   new_lines_opt   command   pipeline_tail
            pipeline.Rule =
                assignment_expression | _pipeline_expression | _pipeline_command
                ;

            _pipeline_expression.Rule = expression + redirections_opt + pipeline_tails;
            _pipeline_command.Rule = command + pipeline_tails;

            pipeline_tails.Rule = MakeStarRule(pipeline_tails, pipeline_tail);
            pipeline_tail.Rule = "|" + command;

            ////        assignment_expression:
            ////            expression   assignment_operator   statement
            //
            // I think the left side should be `primary_expression`, not `expression`.
            assignment_expression.Rule =
                primary_expression + assignment_operator + statement;


            ////        command:
            ////            command_name   command_elements_opt
            ////            command_invocation_operator   command_module_opt  command_name_expr   command_elements_opt
            command.Rule =
                _command_simple | _command_invocation;

            _command_simple.Rule =
                command_name + command_elements_opt;

            // ISSUE: https://github.com/Pash-Project/Pash/issues/8
            _command_invocation.Rule =
                command_invocation_operator + /* command_module_opt + */ command_name_expr + command_elements_opt;

            ////        command_invocation_operator:  one of
            ////            &	.
            command_invocation_operator.Rule =
                ToTerm("&") | ".";

            ////        command_module:
            ////            primary_expression
            // ISSUE: https://github.com/Pash-Project/Pash/issues/8
            command_module.Rule =
                primary_expression;

            ////        command_name:
            ////            generic_token
            ////            generic_token_with_subexpr
            command_name.Rule =
                generic_token
                // ISSUE: https://github.com/Pash-Project/Pash/issues/9 - need whitespace prohibition
                // |
                // generic_token_with_subexpr
                ;

            ////        generic_token_with_subexpr:
            ////            No whitespace is allowed between ) and command_name.
            ////            generic_token_with_subexpr_start   statement_list_opt   )   command_name
            // ISSUE: https://github.com/Pash-Project/Pash/issues/9 - need whitespace prohibition
            generic_token_with_subexpr.Rule =
                generic_token_with_subexpr_start + statement_list_opt + ")" + command_name;

            ////        command_name_expr:
            ////            command_name
            ////            primary_expression
            command_name_expr.Rule =
                command_name
                |
                primary_expression
                ;

            ////        command_elements:
            ////            command_element
            ////            command_elements   command_element
            command_elements.Rule =
                MakePlusRule(command_elements, command_element);
            command_elements_opt.SetFlag(TermFlags.IsTransient, false);

            ////        command_element:
            ////            command_parameter
            ////            command_argument
            ////            redirection
            command_element.Rule =
                command_parameter
                |
                command_argument
                |
                redirection
                ;

            ////        command_argument:
            ////            command_name_expr
            // Deviation from the official language spec here, to allow arrays
            command_argument.Rule =
                MakePlusRule(command_argument, ToTerm(","), command_name_expr);

            ////        redirections:
            ////            redirection
            ////            redirections   redirection
            redirections.Rule =
                MakePlusRule(redirections, redirection);

            ////        redirection:
            ////            2>&1
            ////            1>&2
            ////            file_redirection_operator   redirected_file_name
            redirection.Rule = _redirection_error_to_output | _redirection_reserved | (file_redirection_operator + redirected_file_name);
            _redirection_error_to_output.Rule = ToTerm("2>&1");
            _redirection_reserved.Rule = ToTerm("1>&2");

            ////        redirected_file_name:
            ////            command_argument
            ////            primary_expression
            // I think there's a bug here in the grammar, as command_argument already points to primary_expression.
            redirected_file_name.Rule =
                command_argument
                // | primary_expression
                ;
            #endregion

            #region B.2.3 Expressions
            ////        expression:
            ////            logical_expression
            expression.Rule =
                logical_expression;

            ////        logical_expression:
            ////            bitwise_expression
            ////            logical_expression   _and   new_lines_opt   bitwise_expression
            ////            logical_expression   _or   new_lines_opt   bitwise_expression
            ////            logical_expression   _xor   new_lines_opt   bitwise_expression
            logical_expression.Rule =
                bitwise_expression
                |
                (logical_expression + _logical_expression_operator + bitwise_expression)
                ;
            _logical_expression_operator.Rule = _operator_and | _operator_or | _operator_xor;

            ////        bitwise_expression:
            ////            comparison_expression
            ////            bitwise_expression   _band   new_lines_opt   comparison_expression
            ////            bitwise_expression   _bor   new_lines_opt   comparison_expression
            ////            bitwise_expression   _bxor   new_lines_opt   comparison_expression
            bitwise_expression.Rule =
                comparison_expression
                |
                (bitwise_expression + _bitwise_expression_operator + comparison_expression)
                ;
            _bitwise_expression_operator.Rule = _operator_band | _operator_bor | _operator_bxor;

            ////        comparison_expression:
            ////            additive_expression
            ////            comparison_expression   comparison_operator   new_lines_opt   additive_expression
            comparison_expression.Rule =
                additive_expression
                |
                (comparison_expression + comparison_operator + additive_expression)
                ;

            ////        additive_expression:
            ////            multiplicative_expression
            ////            additive_expression   +   new_lines_opt   multiplicative_expression
            ////            additive_expression   dash   new_lines_opt   multiplicative_expression
            additive_expression.Rule =
                multiplicative_expression
                |
                (additive_expression + _additive_expression_operator + multiplicative_expression)
                ;
            _additive_expression_operator.Rule = "+" | dash;

            ////        multiplicative_expression:
            ////            format_expression
            ////            multiplicative_expression   *   new_lines_opt   format_expression
            ////            multiplicative_expression   /   new_lines_opt   format_expression
            ////            multiplicative_expression   %   new_lines_opt   format_expression
            multiplicative_expression.Rule =
                format_expression
                |
                (multiplicative_expression + _multiplicative_expression_operator + format_expression)
                ;
            _multiplicative_expression_operator.Rule = ToTerm("*") | ToTerm("/") | ToTerm("%");

            ////        format_expression:
            ////            range_expression
            ////            format_expression   format_operator    new_lines_opt   range_expression
            format_expression.Rule =
                range_expression
                |
                (format_expression + format_operator + range_expression)
                ;

            ////        range_expression:
            ////            array_literal_expression
            ////            range_expression   ..   new_lines_opt   array_literal_expression
            range_expression.Rule =
                array_literal_expression
                |
                (range_expression + ".." + array_literal_expression)
                ;

            ////        array_literal_expression:
            ////            unary_expression
            ////            unary_expression   ,    new_lines_opt   array_literal_expression
            // ISSUE 367 https://github.com/Pash-Project/Pash/issues/367
            // A custom action is used to not allow literal arrays in a parameter list default value
            array_literal_expression.Rule = 
                unary_expression
                |
                (unary_expression + CustomActionHere(ResolveLiteralArrayConflict) + "," + array_literal_expression)
                ;

            ////        unary_expression:
            ////            primary_expression
            ////            expression_with_unary_operator
            unary_expression.Rule =
                primary_expression
                |
                expression_with_unary_operator
                ;

            ////        expression_with_unary_operator:
            ////            ,   new_lines_opt   unary_expression
            ////            -not   new_lines_opt   unary_expression
            ////            !   new_lines_opt   unary_expression
            ////            -bnot   new_lines_opt   unary_expression
            ////            +   new_lines_opt   unary_expression
            ////            dash   new_lines_opt   unary_expression
            ////            pre_increment_expression
            ////            pre_decrement_expression
            ////            cast_expression
            ////            -split   new_lines_opt   unary_expression
            ////            -join   new_lines_opt   unary_expression
            ///

            // TODO: the following expresion covers all the different cases mentioned above as the original
            // implementation increases the creation time of the grammar extremely (about 5 seconds)
            // If someone has good ideas and is an grammar pro, feel free to change this again!
            expression_with_unary_operator.Rule = cast_expression | _joined_unary_operator_expression;
            _joined_unary_operator_expression.Rule = _unary_operator_expression + unary_expression;
            _unary_operator_expression.Rule =
                _operator_not
                |
                ","
                |
                "!"
                |
                _operator_bnot
                |
                "+"
                |
                dash
                |
                "-split"
                |
                "-join"
                |
                "++"
                |
                dashdash;

            /* See Pash issue #214
             * Original code that splits the rule in different expressions:

            expression_with_unary_operator.Rule =
                _unary_array_expression
                |
                _unary_not_expression
                |
                _unary_bang_expression
                |
                _unary_bnot_expression
                |
                _unary_plus_expression
                |
                _unary_dash_expression
                |
                pre_increment_expression
                |
                pre_decrement_expression
                |
                cast_expression
                |
                _unary_split_expression
                |
                _unary_join_expression
                ;
            _unary_array_expression.Rule = "," + unary_expression;
            _unary_not_expression.Rule = _operator_not + unary_expression;
            _unary_bang_expression.Rule = "!" + unary_expression;
            _unary_bnot_expression.Rule = _operator_bnot + unary_expression;
            _unary_plus_expression.Rule = "+" + unary_expression;
            _unary_dash_expression.Rule = dash + unary_expression;
            _unary_split_expression.Rule = "-split" + unary_expression;
            _unary_join_expression.Rule = "-join" + unary_expression;

            ////        pre_increment_expression:
            ////            ++   new_lines_opt   unary_expression
            pre_increment_expression.Rule =
                "++" + unary_expression;

            ////        pre_decrement_expression:
            ////            dashdash   new_lines_opt   unary_expression
            pre_decrement_expression.Rule =
                dashdash + unary_expression;
            */
            ////        cast_expression:
            ////            type_literal   unary_expression
            cast_expression.Rule =
                type_literal + unary_expression;

            ////        primary_expression:
            ////            value
            ////            member_access
            ////            element_access
            ////            invocation_expression
            ////            post_increment_expression
            ////            post_decrement_expression
            primary_expression.Rule =
                value
                |
                _member_access_or_invocation_expression
                |
                element_access
                |
                post_increment_expression
                |
                post_decrement_expression
                ;

            ////        value:
            ////            parenthesized_expression
            ////            sub_expression
            ////            array_expression
            ////            script_block_expression
            ////            hash_literal_expression
            ////            literal
            ////            type_literal
            ////            variable
            value.Rule = parenthesized_expression
                |
                sub_expression
                |
                array_expression
                |
                script_block_expression
                |
                hash_literal_expression
                |
                literal
                |
                type_literal
                |
                variable
                ;

            ////        parenthesized_expression:
            ////            (   new_lines_opt   pipeline   new_lines_opt   )
            parenthesized_expression.Rule =
                ToTerm("(") + pipeline + ")";

            ////        sub_expression:
            ////            $(   new_lines_opt   statement_list_opt   new_lines_opt   )
            sub_expression.Rule =
                "$(" + statement_list_opt + ")";

            ////        array_expression:
            ////            @(   new_lines_opt   statement_list_opt   new_lines_opt   )
            array_expression.Rule =
                "@(" + statement_list_opt + ")";

            ////        script_block_expression:
            ////            {   new_lines_opt   script_block   new_lines_opt   }
            script_block_expression.Rule =
                "{" + script_block + "}";

            ////        hash_literal_expression:
            ////            @{   new_lines_opt   hash_literal_body_opt   new_lines_opt   }
            hash_literal_expression.Rule =
                ToTerm("@{") + hash_literal_body_opt + "}";

            ////        hash_literal_body:
            ////            hash_entry
            ////            hash_literal_body   statement_terminators   hash_entry
            hash_literal_body.Rule =
                MakeListRule(hash_literal_body, statement_terminators, hash_entry, TermListOptions.PlusList | TermListOptions.AllowTrailingDelimiter);

            ////        hash_entry:
            ////            key_expression   =   new_lines_opt   statement
            hash_entry.Rule =
                key_expression + "=" + statement;

            ////        key_expression:
            ////            simple_name
            ////            unary_expression
            key_expression.Rule =
                simple_name
                |
                unary_expression
                ;

            ////        post_increment_expression:
            ////            primary_expression   ++
            post_increment_expression.Rule =
                primary_expression + "++";

            ////        post_decrement_expression:
            ////            primary_expression   dashdash
            post_decrement_expression.Rule =
                primary_expression + dashdash;

            ////        member_access: Note no whitespace is allowed between terms in these productions.
            ////            primary_expression   .   member_name
            ////            primary_expression   ::   member_name
            ////        invocation_expression: Note no whitespace is allowed between terms in these productions.
            ////            primary_expression   .   member_name   argument_list
            ////            primary_expression   ::   member_name   argument_list
            ////        argument_list:
            ////            (   argument_expression_list_opt   new_lines_opt   )

            _member_access_or_invocation_expression.Rule =
                primary_expression + _member_access_or_invocation_expression_operator + member_name + PreferShiftHere() + "(" + argument_expression_list_opt + ")"
                |
                primary_expression + _member_access_or_invocation_expression_operator + member_name
                ;

            _member_access_or_invocation_expression.Reduced += delegate(object sender, ReducedEventArgs e)
            {
                if (!AreTerminalsContiguous(e.ResultNode.ChildNodes[0], e.ResultNode.ChildNodes[1]))
                {
                    e.Context.AddParserError("unexpected whitespace", e.ResultNode.ChildNodes[0].Span.EndLocation());
                }
            };

            _member_access_or_invocation_expression_operator.Rule = ToTerm(".") | "::";

            ////        element_access: Note no whitespace is allowed between primary_expression and [.
            ////            primary_expression   [  new_lines_opt   expression   new_lines_opt   ]
            element_access.Rule =
                primary_expression + PreferShiftHere() + "[" + expression + "]";

            element_access.Reduced += delegate(object sender, ReducedEventArgs e)
            {
                if (!AreTerminalsContiguous(e.ResultNode.ChildNodes[0], e.ResultNode.ChildNodes[1]))
                {
                    e.Context.AddParserError("unexpected whitespace", e.ResultNode.ChildNodes[0].Span.EndLocation());
                }
            };

            ////        argument_expression_list:
            ////            argument_expression
            ////            argument_expression   new_lines_opt   ,   argument_expression_list
            argument_expression_list.Rule =
                MakePlusRule(argument_expression_list, ToTerm(","), argument_expression);

            ////        argument_expression:
            ////            new_lines_opt   logical_argument_expression
            argument_expression.Rule =
                 logical_argument_expression;

            ////        logical_argument_expression:
            ////            bitwise_argument_expression
            ////            logical_argument_expression   _and   new_lines_opt   bitwise_argument_expression
            ////            logical_argument_expression   _or   new_lines_opt   bitwise_argument_expression
            ////            logical_argument_expression   _xor   new_lines_opt   bitwise_argument_expression
            logical_argument_expression.Rule =
                bitwise_argument_expression
                |
                logical_argument_expression + _logical_expression_operator + bitwise_argument_expression
                ;

            ////        bitwise_argument_expression:
            ////            comparison_argument_expression
            ////            bitwise_argument_expression   _band   new_lines_opt   comparison_argument_expression
            ////            bitwise_argument_expression   _bor   new_lines_opt   comparison_argument_expression
            ////            bitwise_argument_expression   _bxor   new_lines_opt   comparison_argument_expression
            bitwise_argument_expression.Rule =
                comparison_argument_expression
                |
                bitwise_argument_expression + _bitwise_expression_operator + comparison_argument_expression
                ;

            ////        comparison_argument_expression:
            ////            additive_argument_expression
            ////            comparison_argument_expression   comparison_operator
            ////                        new_lines_opt   additive_argument_expression
            comparison_argument_expression.Rule =
                additive_argument_expression
                |
                comparison_argument_expression + comparison_operator +
                             additive_argument_expression
                            ;

            ////        additive_argument_expression:
            ////            multiplicative_argument_expression
            ////            additive_argument_expression   +   new_lines_opt   multiplicative_argument_expression
            ////            additive_argument_expression   dash   new_lines_opt   multiplicative_argument_expression
            additive_argument_expression.Rule =
                multiplicative_argument_expression
                |
                (additive_argument_expression + _additive_expression_operator + multiplicative_argument_expression)
                ;

            ////        multiplicative_argument_expression:
            ////            format_argument_expression
            ////            multiplicative_argument_expression   *   new_lines_opt   format_argument_expression
            ////            multiplicative_argument_expression   /   new_lines_opt   format_argument_expression
            ////            multiplicative_argument_expression   %   new_lines_opt   format_argument_expression
            multiplicative_argument_expression.Rule =
                format_argument_expression
                |
                (multiplicative_argument_expression + _multiplicative_expression_operator + format_argument_expression)
                ;

            ////        format_argument_expression:
            ////            range_argument_expression
            ////            format_argument_expression   format_operator   new_lines_opt   range_argument_expression
            format_argument_expression.Rule =
                range_argument_expression
                |
                (format_argument_expression + format_operator + range_argument_expression)
                ;

            ////        range_argument_expression:
            ////            unary_expression
            ////            range_expression   ..   new_lines_opt   unary_expression
            // See https://github.com/Pash-Project/Pash/issues/51
            range_argument_expression.Rule =
                unary_expression
                |
                (unary_expression + ".." + unary_expression)
                ;

            ////        member_name:
            ////            simple_name
            ////            string_literal
            ////            string_literal_with_subexpression
            ////            expression_with_unary_operator
            ////            value
            // NOTE: here is another error in the PS grammar:
            // It doesn't make sense to use a value or string_literal or string_literal_with_subexpression
            // since "value" contains "literal" which contains both forms of string_literal.
            // Stating them here explicitly (again) yields to a reduce/reduce conflicht since the string_literal
            // could be reduced to literal->value->member_name or directly.
            // So we leave them out and only regard them as a value
            member_name.Rule =
                simple_name
                |
                // TODO: 
                // |
                // expression_with_unary_operator
                // |
                value
                ;

            ////        string_literal_with_subexpression:
            ////            expandable_string_literal_with_subexpr
            ////            expandable_here_string_literal_with_subexpr
            // TODO: expandable_here_string_literal_with_subexpr
            string_literal_with_subexpression.Rule =
                expandable_string_literal_with_subexpr;

            ////        expandable_string_literal_with_subexpr:
            ////            expandable_string_with_subexpr_start   statement_list_opt   )
            ////                    expandable_string_with_subexpr_characters   expandable_string_with_subexpr_end
            ////            expandable_here_string_with_subexpr_start   statement_list_opt   )
            ////                    expandable_here_string_with_subexpr_characters
            ////                    expandable_here_string_with_subexpr_end
            // TODO: expandable_here_string_with_subexpr_start
            expandable_string_literal_with_subexpr.Rule =
                expandable_string_with_subexpr_start + statement_list_opt + ")" +
                    expandable_string_with_subexpr_characters_opt + expandable_string_with_subexpr_end;

            ////        expandable_string_with_subexpr_characters:
            ////            expandable_string_with_subexpr_part
            ////            expandable_string_with_subexpr_characters   expandable_string_with_subexpr_part
            expandable_string_with_subexpr_characters.Rule =
                MakePlusRule(expandable_string_with_subexpr_characters, expandable_string_with_subexpr_part);

            ////        expandable_string_with_subexpr_part:
            ////            sub_expression
            ////            expandable_string_part
            expandable_string_with_subexpr_part.Rule =
                sub_expression
                |
                //expandable_string_part        // single character
                expandable_string_characters    // n character
                ;

            ////        expandable_here_string_with_subexpr_characters:
            ////            expandable_here_string_with_subexpr_part
            ////            expandable_here_string_with_subexpr_characters   expandable_here_string_with_subexpr_part
            ////        expandable_here_string_with_subexpr_part:
            ////            sub_expression
            ////            expandable_here_string_part

            ////        type_literal:
            ////            [    type_spec   ]
            type_literal.Rule =
                "[" + type_spec + "]";

            ////        type_spec:
            ////            array_type_name    dimension_opt   ]
            ////            generic_type_name   generic_type_arguments   ]
            ////            type_name
            ////        dimension:
            ////            ,
            ////            dimension   ,
            type_spec.Rule =
                // TODO:
                _type_spec_array
                |
                //_type_spec_generic
                //|
                type_name
                ;

            _type_spec_array.Rule = array_type_name + _type_spec_array_separators + "]";
            _type_spec_array_separators.Rule = MakeStarRule(_type_spec_array_separators, ToTerm(","));

            _type_spec_generic.Rule = generic_type_name + generic_type_arguments + "]";

            ////        generic_type_arguments:
            ////            type_spec
            ////            generic_type_arguments   ,   type_spec
            generic_type_arguments.Rule = MakePlusRule(generic_type_arguments, ToTerm(","), type_spec);

            #endregion

            #region B.2.4 Attributes
            ////        attribute_list:
            ////            attribute
            ////            attribute_list   new_lines_opt   attribute
            attribute_list.Rule =
                MakePlusRule(attribute_list, attribute);

            ////        attribute:
            ////            [   attribute_name   (   attribute_arguments   new_lines_opt   )  new_lines_opt   ]
            ////            type_literal
            attribute.Rule =
                ("[" + attribute_name + "(" + attribute_arguments + ")" + "]")
                |
                type_literal
                ;

            ////        attribute_name:
            ////            type_spec
            attribute_name.Rule =
                type_spec;

            ////        attribute_arguments:
            ////            attribute_argument
            ////            attribute_argument   new_lines_opt   ,   attribute_arguments
            attribute_arguments.Rule = MakePlusRule(attribute_arguments, ToTerm(","), attribute_argument);

            ////        attribute_argument:
            ////            new_lines_opt   expression
            ////            new_lines_opt   simple_name   =   new_lines_opt   expression
            attribute_argument.Rule =
                (expression)
                |
                (simple_name + "=" + expression)
                ;
            #endregion
            #endregion
        }

        #region Context-sensitive resolving of shift/reduce conflict with parameter lists and literal arrays

        // Only parse a literal array if we're not inside a pameter list. We check the context sensitivity by
        // using implict beginParamList and endParamList symbols
        void ResolveLiteralArrayConflict(ParsingContext context, CustomParserAction customAction)
        {
            // First check for a comma term. Any other is not a list, so it must be a reduction to unary_expression
            if (context.CurrentParserInput.Term.Name != ",")
            {
                customAction.ReduceActions.First().Execute(context);
                return;
            }
            // if there is no possibility to reduce, just do a shift
            var firstCorrectShiftAction = customAction.ShiftActions.First(a => a.Term.Name == ",");
            if (customAction.ReduceActions.Count < 1)
            {
                firstCorrectShiftAction.Execute(context);
                return;
            }
            // so we can shift or reduce. Let's look if we're in a paramBlock
            // we do this be iterating over the read tokens backwards and checking for a marker token which marks
            // the beginning and end of a parameter list
            var tokens = context.CurrentParseTree.Tokens;
            var isParamList = false;
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                var tk = tokens[i];
                if (tk.Terminal == _begin_paramlist_marker)
                {
                    isParamList = true; // yes, inside a param block
                    break;
                }
                else if (tk.Terminal == _end_paramlist_marker)
                {
                    break; // we're outside a param block
                }
            }

            // if we're inside a parameter list, reduce (so not array literal is parsed)
            if (isParamList)
            {
                customAction.ReduceActions.First().Execute(context);
                return;
            }
            // otherwise just shift to parse the array literal
            firstCorrectShiftAction.Execute(context);
        }

        #endregion

        bool AreTerminalsContiguous(ParseTreeNode parseTreeNode1, ParseTreeNode parseTreeNode2)
        {
            return SourceLocation.Compare(parseTreeNode1.Span.EndLocation(), parseTreeNode2.Span.Location) == 0;
        }

        // returns the number of characters to skip
        int SkipSingleWhitespace(ISourceStream source)
        {
            switch (CharUnicodeInfo.GetUnicodeCategory(source.PreviewChar))
            {
                ////        whitespace:
                ////            Any character with Unicode class Zs, Zl, or Zp
                case UnicodeCategory.SpaceSeparator:        // Zs
                case UnicodeCategory.LineSeparator:         // Zl
                case UnicodeCategory.ParagraphSeparator:    // Zp
                    return 1;
            }

            switch (source.PreviewChar)
            {
                ////            Horizontal tab character (U+0009)
                case '\u0009':
                ////            Vertical tab character (U+000B)
                case '\u000B':
                ////            Form feed character (U+000C)
                case '\u000C':
                    return 1;
            }

            return 0;
        }

        public override void SkipWhitespace(ISourceStream source)
        {
            while (!source.EOF())
            {
                int count = SkipSingleWhitespace(source);

                if (count == 0) return;

                else source.PreviewPosition += count;
            }
        }

        void InitializeNonTerminalFields()
        {
            var nonTerminalFields = from field in this.GetType().GetFields()
                                    where field.FieldType == typeof(NonTerminal)
                                    select field;

            foreach (var field in nonTerminalFields)
            {
                Debug.Assert(field.GetValue(this) == null, "Don't pre-initalize the NonTerminal fields. We'll do that in reflection");

                var nonTerminal = new NonTerminal(field.Name);

                field.SetValue(this, nonTerminal);
            }

            foreach (var field in nonTerminalFields.Where(f => f.Name.EndsWith("_opt")))
            {
                var parentFieldName = field.Name.Remove(field.Name.Length - "_opt".Length);
                NonTerminal nonTerminal = (NonTerminal)field.GetValue(this);
                BnfTerm term = (BnfTerm)nonTerminalFields.Single(f => f.Name == parentFieldName).GetValue(this);

                nonTerminal.Rule = term | Empty;
                MarkTransient(nonTerminal);
            }
        }
    }
}

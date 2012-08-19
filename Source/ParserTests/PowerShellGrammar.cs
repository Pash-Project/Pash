using System;
using System.Collections.Generic;
using System.Linq;

using Irony.Parsing;
using System.Text.RegularExpressions;
using System.Text;

namespace ParserTests
{
    ////////
    // PowerShell Language Syntactic Grammar, as presented in the PowerShell Language Specification[1], Appendix B.2
    //
    // [1]: http://www.microsoft.com/en_us/download/details.aspx?id=9706
    ///////

    partial class PowerShellGrammar : CaseInsensitiveGrammar
    {

        #region B.2 Syntactic grammar

        #region B.2.1 Basic concepts
        public readonly NonTerminal script_file;
        public readonly NonTerminal module_file;
        public readonly NonTerminal interactive_input;
        public readonly NonTerminal data_file;
        #endregion

        #region B.2.2 Statements
        public readonly NonTerminal script_block;
        public readonly NonTerminal param_block;
        public readonly NonTerminal parameter_list;
        public readonly NonTerminal script_parameter;
        public readonly NonTerminal script_parameter_default;
        public readonly NonTerminal script_block_body;
        public readonly NonTerminal named_block_list;
        public readonly NonTerminal named_block;
        public readonly NonTerminal block_name;
        public readonly NonTerminal statement_block;
        public readonly NonTerminal statement_list;
        public readonly NonTerminal statement;
        public readonly NonTerminal statement_terminator;
        public readonly NonTerminal statement_terminators;
        public readonly NonTerminal if_statement;
        public readonly NonTerminal elseif_clauses;
        public readonly NonTerminal elseif_clause;
        public readonly NonTerminal else_clause;
        public readonly NonTerminal labeled_statement;
        public readonly NonTerminal switch_statement;
        public readonly NonTerminal switch_parameters;
        public readonly NonTerminal switch_parameter;
        public readonly NonTerminal switch_condition;
        public readonly NonTerminal switch_filename;
        public readonly NonTerminal switch_body;
        public readonly NonTerminal switch_clauses;
        public readonly NonTerminal switch_clause;
        public readonly NonTerminal switch_clause_condition;
        public readonly NonTerminal foreach_statement;
        public readonly NonTerminal for_statement;
        public readonly NonTerminal for_initializer;
        public readonly NonTerminal for_condition;
        public readonly NonTerminal for_iterator;
        public readonly NonTerminal while_statement;
        public readonly NonTerminal do_statement;
        public readonly NonTerminal while_condition;
        public readonly NonTerminal function_statement;
        public readonly NonTerminal function_name;
        public readonly NonTerminal function_parameter_declaration;
        public readonly NonTerminal flow_control_statement;
        public readonly NonTerminal label_expression;
        public readonly NonTerminal trap_statement;
        public readonly NonTerminal try_statement;
        public readonly NonTerminal catch_clauses;
        public readonly NonTerminal catch_clause;
        public readonly NonTerminal catch_type_list;
        public readonly NonTerminal finally_clause;
        public readonly NonTerminal data_statement;
        public readonly NonTerminal data_name;
        public readonly NonTerminal data_commands_allowed;
        public readonly NonTerminal data_commands_list;
        public readonly NonTerminal data_command;
        public readonly NonTerminal pipeline;
        public readonly NonTerminal assignment_expression;
        public readonly NonTerminal pipeline_tail;
        public readonly NonTerminal command;
        public readonly NonTerminal command_invocation_operator;
        public readonly NonTerminal command_module;
        public readonly NonTerminal command_name;
        public readonly NonTerminal generic_token_with_subexpr;
        public readonly NonTerminal command_name_expr;
        public readonly NonTerminal command_elements;
        public readonly NonTerminal command_element;
        public readonly NonTerminal command_argument;
        public readonly NonTerminal redirections;
        public readonly NonTerminal redirection;
        public readonly NonTerminal redirected_file_name;
        #endregion

        #region B.2.3 Expressions
        public readonly NonTerminal expression;
        public readonly NonTerminal logical_expression;
        public readonly NonTerminal bitwise_expression;
        public readonly NonTerminal comparison_expression;
        public readonly NonTerminal additive_expression;
        public readonly NonTerminal multiplicative_expression;
        public readonly NonTerminal format_expression;
        public readonly NonTerminal range_expression;
        public readonly NonTerminal array_literal_expression;
        public readonly NonTerminal unary_expression;
        public readonly NonTerminal expression_with_unary_operator;
        public readonly NonTerminal pre_increment_expression;
        public readonly NonTerminal pre_decrement_expression;
        public readonly NonTerminal cast_expression;
        public readonly NonTerminal primary_expression;
        public readonly NonTerminal value;
        public readonly NonTerminal parenthesized_expression;
        public readonly NonTerminal sub_expression;
        public readonly NonTerminal array_expression;
        public readonly NonTerminal script_block_expression;
        public readonly NonTerminal hash_literal_expression;
        public readonly NonTerminal hash_literal_body;
        public readonly NonTerminal hash_entry;
        public readonly NonTerminal key_expression;
        public readonly NonTerminal post_increment_expression;
        public readonly NonTerminal post_decrement_expression;
        public readonly NonTerminal member_access;
        public readonly NonTerminal element_access;
        public readonly NonTerminal invocation_expression;
        public readonly NonTerminal argument_list;
        public readonly NonTerminal argument_expression_list;
        public readonly NonTerminal argument_expression;
        public readonly NonTerminal logical_argument_expression;
        public readonly NonTerminal bitwise_argument_expression;
        public readonly NonTerminal comparison_argument_expression;
        public readonly NonTerminal additive_argument_expression;
        public readonly NonTerminal multiplicative_argument_expression;
        public readonly NonTerminal format_argument_expression;
        public readonly NonTerminal range_argument_expression;
        public readonly NonTerminal member_name;
        public readonly NonTerminal string_literal_with_subexpression;
        public readonly NonTerminal expandable_string_literal_with_subexpr;
        public readonly NonTerminal expandable_string_with_subexpr_characters;
        public readonly NonTerminal expandable_string_with_subexpr_part;
        public readonly NonTerminal expandable_here_string_with_subexpr_characters;
        public readonly NonTerminal expandable_here_string_with_subexpr_part;
        public readonly NonTerminal type_literal;
        public readonly NonTerminal type_spec;
        public readonly NonTerminal dimension;
        public readonly NonTerminal generic_type_arguments;
        #endregion

        #region B.2.4 Attributes
        public readonly NonTerminal attribute_list;
        public readonly NonTerminal attribute;
        public readonly NonTerminal attribute_name;
        public readonly NonTerminal attribute_arguments;
        public readonly NonTerminal attribute_argument;
        #endregion
        #endregion

        public class ScriptFile : PowerShellGrammar
        {
            public ScriptFile()
            {
                Root = this.script_file;
            }
        }

        public class ModuleFile : PowerShellGrammar
        {
            public ModuleFile()
            {
                Root = this.module_file;
            }
        }

        public class InteractiveInput : PowerShellGrammar
        {
            public InteractiveInput()
            {
                Root = this.interactive_input;
            }
        }

        public class DataFile : PowerShellGrammar
        {
            public DataFile()
            {
                Root = this.data_file;
            }
        }

        PowerShellGrammar()
        {
            InitializeNonTerminals();

            // delegate to a new method, so we don't accidentally overwrite a readonly field.
            InitializeProductionRules();

        }

        public void InitializeProductionRules()
        {
            #region B.2 Syntactic grammar

            #region B.2.1 Basic concepts
            ////        script_file:
            ////            script_block
            ////        module_file:
            ////            script_block
            ////        interactive_input:
            ////            script_block
            interactive_input.Rule = script_block;

            ////        data_file:
            ////            statement_list
            #endregion

            #region B.2.2 Statements
            ////        script_block:
            ////            param_block_opt   statement_terminators_opt    script_block_body_opt
            // TODO: more
            script_block.Rule = script_block_body;

            ////        param_block:
            ////            new_lines_opt   attribute_list_opt   new_lines_opt   param   new_lines_opt
            ////                    (   parameter_list_opt   new_lines_opt   )
            ////        parameter_list:
            ////            script_parameter
            ////            parameter_list   new_lines_opt   ,   script_parameter
            ////        script_parameter:
            ////            new_lines_opt   attribute_list_opt   new_lines_opt   variable   script_parameter_default_opt
            ////        script_parameter_default:
            ////            new_lines_opt   =   new_lines_opt   expression
            ////        script_block_body:
            ////            named_block_list
            ////            statement_list
            // TODO: more
            script_block_body.Rule = statement_list;

            ////        named_block_list:
            ////            named_block
            ////            named_block_list   named_block
            ////        named_block:
            ////            block_name   statement_block   statement_terminators_opt
            ////        block_name:  one of
            ////            dynamicparam   begin   process   end
            ////        statement_block:
            ////            new_lines_opt   {   statement_list_opt   new_lines_opt   }
            ////        statement_list:
            ////            statement
            ////            statement_list   statement
            statement_list.Rule = MakePlusRule(statement_list, statement);

            ////        statement:
            ////            if_statement
            ////            label_opt   labeled_statement
            ////            function_statement
            ////            flow_control_statement   statement_terminator
            ////            trap_statement
            ////            try_statement
            ////            data_statement
            ////            pipeline   statement_terminator
            // TODO: more
            statement.Rule = pipeline;

            ////        statement_terminator:
            ////            ;
            ////            new_line_character
            ////        statement_terminators:
            ////            statement_terminator
            ////            statement_terminators   statement_terminator
            ////        if_statement:
            ////            if   new_lines_opt   (   new_lines_opt   pipeline   new_lines_opt   )   statement_block
            ////                     elseif_clauses_opt   else_clause_opt
            ////        elseif_clauses:
            ////            elseif_clause
            ////            elseif_clauses   elseif_clause
            ////        elseif_clause:
            ////            new_lines_opt   elseif   new_lines_opt   (   new_lines_opt   pipeline   new_lines_opt   )   statement_block
            ////        else_clause:
            ////            new_lines_opt   else   statement_block
            ////        labeled_statement:
            ////            switch_statement
            ////            foreach_statement
            ////            for_statement
            ////            while_statement
            ////            do_statement
            ////        switch_statement:
            ////            switch   new_lines_opt   switch_parameters_opt   switch_condition   switch_body
            ////        switch_parameters:
            ////            switch_parameter
            ////            switch_parameters   switch_parameter
            ////        switch_parameter:
            ////            _regex
            ////            _wildcard
            ////            _exact
            ////            _casesensitive
            ////        switch_condition:
            ////            (   new_lines_opt   pipeline   new_lines_opt   )
            ////            _file   new_lines_opt   switch_filename
            ////        switch_filename:
            ////            command_argument
            ////            primary_expression
            ////        switch_body:
            ////            new_lines_opt   {   new_lines_opt   switch_clauses   }
            ////        switch_clauses:
            ////            switch_clause
            ////            switch_clauses   switch_clause
            ////        switch_clause:
            ////            switch_clause_condition   statement_block   statement_terimators_opt
            ////        switch_clause_condition:
            ////            command_argument
            ////            primary_expression
            ////        foreach_statement:
            ////            foreach   new_lines_opt   (   new_lines_opt   variable   new_lines_opt   in   new_lines_opt   pipeline
            ////                    new_lines_opt   )   statement_block
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
            ////        for_initializer:
            ////            pipeline
            ////        for_condition:
            ////            pipeline
            ////        for_iterator:
            ////            pipeline
            ////        while_statement:
            ////            while   new_lines_opt   (   new_lines_opt   while_condition   new_lines_opt   )   statement_block
            ////        do_statement:
            ////            do   statement_block  new_lines_opt   while   new_lines_opt   (   while_condition   new_lines_opt   )
            ////            do   statement_block   new_lines_opt   until   new_lines_opt   (   while_condition   new_lines_opt   )
            ////        while_condition:
            ////            new_lines_opt   pipeline
            ////        function_statement:
            ////            function   new_lines_opt   function_name   function_parameter_declaration_opt   {   script_block   }
            ////            filter   new_lines_opt   function_name   function_parameter_declaration_opt   {   script_block   }
            ////        function_name:
            ////            command_argument
            ////        function_parameter_declaration:
            ////            new_lines_opt   (   parameter_list   new_lines_opt   )
            ////        flow_control_statement:
            ////            break   label_expression_opt
            ////            continue   label_expression_opt
            ////            throw    pipeline_opt
            ////            return   pipeline_opt
            ////            exit   pipeline_opt
            ////        label_expression:
            ////            simple_name
            ////            unary_expression
            ////        trap_statement:
            ////            trap  new_lines_opt   type_literal_opt   new_lines_opt   statement_block
            ////        try_statement:
            ////            try   statement_block   catch_clauses
            ////            try   statement_block   finally_clause
            ////            try   statement_block   catch_clauses   finally_clause
            ////        catch_clauses:
            ////            catch_clause
            ////            catch_clauses   catch_clause
            ////        catch_clause:
            ////            new_lines_opt   catch   catch_type_list_opt   statement_block
            ////        catch_type_list:
            ////            new_lines_opt   type_literal
            ////            catch_type_list   new_lines_opt   ,   new_lines_opt   type_literal
            ////        finally_clause:
            ////            new_lines_opt   finally   statement_block
            ////        data_statement:
            ////            data    new_lines_opt   data_name   data_commands_allowed_opt   statement_block
            ////        data_name:
            ////            simple_name
            ////        data_commands_allowed:
            ////            new_lines_opt   _supportedcommand   data_commands_list
            ////        data_commands_list:
            ////            new_lines_opt   data_command
            ////            data_commands_list   ,   new_lines_opt   data_command
            ////        data_command:
            ////            command_name_expr
            ////        pipeline:
            ////            assignment_expression
            ////            expression   redirections_opt  pipeline_tail_opt
            ////            command   pipeline_tail_opt
            // TODO: more
            pipeline.Rule = command;

            ////        assignment_expression:
            ////            expression   assignment_operator   statement
            ////        pipeline_tail:
            ////            |   new_lines_opt   command
            ////            |   new_lines_opt   command   pipeline_tail
            ////        command:
            ////            command_name   command_elements_opt
            ////            command_invocation_operator   command_module_opt  command_name_expr   command_elements_opt
            // TODO: more
            command.Rule = command_name;

            ////        command_invocation_operator:  one of
            ////            &	.
            ////        command_module:
            ////            primary_expression
            ////        command_name:
            ////            generic_token
            ////            generic_token_with_subexpr
            // TODO: more
            command_name.Rule = Terminals.generic_token;

            ////        generic_token_with_subexpr:
            ////            No whitespace is allowed between ) and command_name.
            ////            generic_token_with_subexpr_start   statement_list_opt   )   command_name
            ////        command_name_expr:
            ////            command_name
            ////            primary_expression
            ////        command_elements:
            ////            command_element
            ////            command_elements   command_element
            ////        command_element:
            ////            command_parameter
            ////            command_argument
            ////            redirection
            ////        command_argument:
            ////            command_name_expr
            ////        redirections:
            ////            redirection
            ////            redirections   redirection
            ////        redirection:
            ////            2>&1
            ////            1>&2
            ////            file_redirection_operator   redirected_file_name
            ////        redirected_file_name:
            ////            command_argument
            ////            primary_expression
            #endregion

            #region B.2.3 Expressions
            ////        expression:
            ////            logical_expression
            ////        logical_expression:
            ////            bitwise_expression
            ////            logical_expression   _and   new_lines_opt   bitwise_expression
            ////            logical_expression   _or   new_lines_opt   bitwise_expression
            ////            logical_expression   _xor   new_lines_opt   bitwise_expression
            ////        bitwise_expression:
            ////            comparison_expression
            ////            bitwise_expression   _band   new_lines_opt   comparison_expression
            ////            bitwise_expression   _bor   new_lines_opt   comparison_expression
            ////            bitwise_expression   _bxor   new_lines_opt   comparison_expression
            ////        comparison_expression:
            ////            additive_expression
            ////            comparison_expression   comparison_operator   new_lines_opt   additive_expression
            ////        additive_expression:
            ////            multiplicative_expression
            ////            additive_expression   +   new_lines_opt   multiplicative_expression
            ////            additive_expression   dash   new_lines_opt   multiplicative_expression
            ////        multiplicative_expression:
            ////            format_expression
            ////            multiplicative_expression   *   new_lines_opt   format_expression
            ////            multiplicative_expression   /   new_lines_opt   format_expression
            ////            multiplicative_expression   %   new_lines_opt   format_expression
            ////        format_expression:
            ////            range_expression
            ////            format_expression   format_operator    new_lines_opt   range_expression
            ////        range_expression:
            ////            array_literal_expression
            ////            range_expression   ..   new_lines_opt   array_literal_expression
            ////        array_literal_expression:
            ////            unary_expression
            ////            unary_expression   ,    new_lines_opt   array_literal_expression
            ////        unary_expression:
            ////            primary_expression
            ////            expression_with_unary_operator
            ////        expression_with_unary_operator:
            ////            ,   new_lines_opt   unary_expression
            ////            _not   new_lines_opt   unary_expression
            ////            !   new_lines_opt   unary_expression
            ////            _bnot   new_lines_opt   unary_expression
            ////            +   new_lines_opt   unary_expression
            ////            dash   new_lines_opt   unary_expression
            ////            pre_increment_expression
            ////            pre_decrement_expression
            ////            cast_expression
            ////            _split   new_lines_opt   unary_expression
            ////            _join   new_lines_opt   unary_expression
            ////        pre_increment_expression:
            ////            ++   new_lines_opt   unary_expression
            ////        pre_decrement_expression:
            ////            dashdash   new_lines_opt   unary_expression
            ////        cast_expression:
            ////            type_literal   unary_expression
            ////        primary_expression:
            ////            value
            ////            member_access
            ////            element_access
            ////            invocation_expression
            ////            post_increment_expression
            ////            post_decrement_expression
            ////        value:
            ////            parenthesized_expression
            ////            sub_expression
            ////            array_expression
            ////            script_block_expression
            ////            hash_literal_expression
            ////            literal
            ////            type_literal
            ////            variable
            ////        parenthesized_expression:
            ////            (   new_lines_opt   pipeline   new_lines_opt   )
            ////        sub_expression:
            ////            $(   new_lines_opt   statement_list_opt   new_lines_opt   )
            ////        array_expression:
            ////            @(   new_lines_opt   statement_list_opt   new_lines_opt   )
            ////        script_block_expression:
            ////            {   new_lines_opt   script_block   new_lines_opt   }
            ////        hash_literal_expression:
            ////            @{   new_lines_opt   hash_literal_body_opt   new_lines_opt   }
            ////        hash_literal_body:
            ////            hash_entry
            ////            hash_literal_body   statement_terminators   hash_entry
            ////        hash_entry:
            ////            key_expression   =   new_lines_opt   statement
            ////        key_expression:
            ////            simple_name
            ////            unary_expression
            ////        post_increment_expression:
            ////            primary_expression   ++
            ////        post_decrement_expression:
            ////            primary_expression   dashdash
            ////        member_access: Note no whitespace is allowed between terms in these productions.
            ////            primary_expression   .   member_name
            ////        primary_expression   ::   member_name
            ////        element_access: Note no whitespace is allowed between primary_expression and [.
            ////            primary_expression   [  new_lines_opt   expression   new_lines_opt   ]
            ////        invocation_expression: Note no whitespace is allowed between terms in these productions.
            ////            primary_expression   .   member_name   argument_list
            ////        primary_expression   ::   member_name   argument_list
            ////        argument_list:
            ////            (   argument_expression_list_opt   new_lines_opt   )
            ////        argument_expression_list:
            ////            argument_expression
            ////            argument_expression   new_lines_opt   ,   argument_expression_list
            ////        argument_expression:
            ////            new_lines_opt   logical_argument_expression
            ////        logical_argument_expression:
            ////            bitwise_argument_expression
            ////            logical_argument_expression   _and   new_lines_opt   bitwise_argument_expression
            ////            logical_argument_expression   _or   new_lines_opt   bitwise_argument_expression
            ////            logical_argument_expression   _xor   new_lines_opt   bitwise_argument_expression
            ////        bitwise_argument_expression:
            ////            comparison_argument_expression
            ////            bitwise_argument_expression   _band   new_lines_opt   comparison_argument_expression
            ////            bitwise_argument_expression   _bor   new_lines_opt   comparison_argument_expression
            ////            bitwise_argument_expression   _bxor   new_lines_opt   comparison_argument_expression
            ////        comparison_argument_expression:
            ////            additive_argument_expression
            ////            comparison_argument_expression   comparison_operator
            ////                        new_lines_opt   additive_argument_expression
            ////        additive_argument_expression:
            ////            multiplicative_argument_expression
            ////            additive_argument_expression   +   new_lines_opt   multiplicative_argument_expression
            ////            additive_argument_expression   dash   new_lines_opt   multiplicative_argument_expression
            ////        multiplicative_argument_expression:
            ////            format_argument_expression
            ////            multiplicative_argument_expression   *   new_lines_opt   format_argument_expression
            ////            multiplicative_argument_expression   /   new_lines_opt   format_argument_expression
            ////            multiplicative_argument_expression   %   new_lines_opt   format_argument_expression
            ////        format_argument_expression:
            ////            range_argument_expression
            ////            format_argument_expression   format_operator   new_lines_opt   range_argument_expression
            ////        range_argument_expression:
            ////            unary_expression
            ////            range_expression   ..   new_lines_opt   unary_expression
            ////        member_name:
            ////            simple _name
            ////            string_literal
            ////            string_literal_with_subexpression
            ////            expression_with_unary_operator
            ////            value
            ////        string_literal_with_subexpression:
            ////            expandable_string_literal_with_subexpr
            ////            expandable_here_string_literal_with_subexpr
            ////        expandable_string_literal_with_subexpr:
            ////            expandable_string_with_subexpr_start   statement_list_opt   )
            ////                    expandable_string_with_subexpr_characters   expandable_string_with_subexpr_end
            ////            expandable_here_string_with_subexpr_start   statement_list_opt   )
            ////                    expandable_here_string_with_subexpr_characters
            ////                    expandable_here_string_with_subexpr_end
            ////        expandable_string_with_subexpr_characters:
            ////            expandable_string_with_subexpr_part
            ////            expandable_string_with_subexpr_characters   expandable_string_with_subexpr_part
            ////        expandable_string_with_subexpr_part:
            ////            sub_expression
            ////            expandable_string_part
            ////        expandable_here_string_with_subexpr_characters:
            ////            expandable_here_string_with_subexpr_part
            ////            expandable_here_string_with_subexpr_characters   expandable_here_string_with_subexpr_part
            ////        expandable_here_string_with_subexpr_part:
            ////            sub_expression
            ////            expandable_here_string_part
            ////        type_literal:
            ////            [    type_spec   ]
            ////        type_spec:
            ////            array_type_name    dimension_opt   ]
            ////            generic_type_name   generic_type_arguments   ]
            ////            type_name
            ////        dimension:
            ////            ,
            ////            dimension   ,
            ////        generic_type_arguments:
            ////            type_spec
            ////            generic_type_arguments   ,   type_spec
            #endregion

            #region B.2.4 Attributes
            ////        attribute_list:
            ////            attribute
            ////            attribute_list   new_lines_opt   attribute
            ////        attribute:
            ////            [   attribute_name   (   attribute_arguments   new_lines_opt   )  new_lines_opt   ]
            ////            type_literal
            ////        attribute_name:
            ////            type_spec
            ////        attribute_arguments:
            ////            attribute_argument
            ////            attribute_argument   new_lines_opt   ,   attribute_arguments
            ////        attribute_argument:
            ////            new_lines_opt   expression
            ////            new_lines_opt   simple_name   =   new_lines_opt   expression
            #endregion
            #endregion
        }

        void InitializeNonTerminals()
        {
            foreach (var field in this.GetType().GetFields().Where(f => f.FieldType == typeof(NonTerminal)))
            {
                if (field.GetValue(this) != null) throw new Exception("don't pre-init fields - let us take care of that for you.");
                field.SetValue(this, new NonTerminal(field.Name));
            }
        }
    }
}

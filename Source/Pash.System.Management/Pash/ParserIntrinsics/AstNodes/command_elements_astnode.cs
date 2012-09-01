using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Management.Automation.Runspaces;

namespace Pash.ParserIntrinsics.AstNodes
{
    */ ------ dead code, fix or delete.
    public class command_elements_astnode : _astnode
    {
        public command_elements_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////        command_elements:
            ////            command_element
            ////            command_elements   command_element
            ////
            ////        command_element:
            ////            command_parameter
            ////            command_argument
            ////            redirection
            ////
            ////        command_argument:
            ////            command_name_expr
            ////
            ////        command_parameter:
            ////            dash   first_parameter_char   parameter_chars   colon_opt

            return ChildAstNodes
                .Select(astNode => astNode.Execute_old(context, commandRuntime))
                .Select(o => new CommandParameter(null, o))
                ;
        }
    }
}

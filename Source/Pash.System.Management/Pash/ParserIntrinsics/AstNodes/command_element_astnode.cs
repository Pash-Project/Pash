using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class command_element_astnode : _astnode
    {
        public readonly command_parameter_astnode Parameter;
        public readonly command_argument_astnode Argument;
        
        public command_element_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command_element:
            ////            command_parameter
            ////            command_argument
            ////            redirection

            if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.command_parameter)
            {
                this.Parameter = this.ChildAstNodes.Single().As<command_parameter_astnode>();
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.command_argument)
            {
                this.Argument = this.ChildAstNodes.Single().As<command_argument_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }
    }
}

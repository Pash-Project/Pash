using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Text.RegularExpressions;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class command_parameter_astnode : _astnode
    {
        public readonly string Name;
        public readonly bool Colon;

        public command_parameter_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command_parameter:
            ////            dash   first_parameter_char   parameter_chars   colon_opt

            Match match = Regex.Match(Text, PowerShellGrammar.Terminals.command_parameter.Pattern);
            this.Name = match.Groups[PowerShellGrammar.Terminals._parameter_name.Name].Value;

            this.Colon = match.Groups[PowerShellGrammar.Terminals.colon.Name].Success;
        }
    }
}

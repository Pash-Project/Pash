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
    public class command_elements_astnode : _astnode
    {
        public readonly IEnumerable<command_element_astnode> Items;

        public command_elements_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        command_elements:
            ////            command_element
            ////            command_elements   command_element

            this.Items = this.ChildAstNodes.Cast<command_element_astnode>();
        }
    }
}

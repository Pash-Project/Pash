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
using System.Collections.ObjectModel;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class hexadecimal_integer_literal_astnode : _astnode
    {
        public readonly int Value;

        public hexadecimal_integer_literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        hexadecimal_integer_literal:
            ////            0x   hexadecimal_digits   long_type_suffix_opt   numeric_multiplier_opt

            this.Value = Convert.ToInt32(Text, 16);
        }
    }
}

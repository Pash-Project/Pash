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
    public class decimal_integer_literal_astnode : _astnode
    {
        public decimal_integer_literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal override object Execute_old(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////        decimal_integer_literal:
            ////            decimal_digits   numeric_type_suffix_opt   numeric_multiplier_opt
            return Convert.ToInt32(Text, 10);
        }
    }
}

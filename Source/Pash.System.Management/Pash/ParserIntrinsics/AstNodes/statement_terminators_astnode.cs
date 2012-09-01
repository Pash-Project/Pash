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
    public class statement_terminators_astnode : _astnode
    {
        public statement_terminators_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            throw new NotImplementedException();
        }
    }
}

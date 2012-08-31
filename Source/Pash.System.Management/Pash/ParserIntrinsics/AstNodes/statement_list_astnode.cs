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
    public class statement_list_astnode : _astnode
    {
        public readonly IEnumerable<statement_astnode> Statements;

        public statement_list_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        statement_list:
            ////            statement
            ////            statement_list   statement

            Statements = this.ChildAstNodes.Cast<statement_astnode>();
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            throw new NotImplementedException();
        }
    }
}

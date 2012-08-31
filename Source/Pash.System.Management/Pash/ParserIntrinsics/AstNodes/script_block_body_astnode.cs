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
    public class script_block_body_astnode : _astnode
    {
        public readonly IEnumerable<statement_astnode> Statements;

        public script_block_body_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        script_block_body:
            ////            named_block_list
            ////            statement_list
            //this.Statements = this.ChildAstNodes.Single().As<statement_list_astnode>().Statements;
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            throw new NotImplementedException();
        }
    }
}

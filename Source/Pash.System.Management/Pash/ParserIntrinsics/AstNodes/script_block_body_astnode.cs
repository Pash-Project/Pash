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

            if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.statement_list)
            {
                this.Statements = this.ChildAstNodes.Single().As<statement_list_astnode>().Statements;
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            List<object> results = new List<object>();

            foreach (var statement in Statements)
            {
                var result = statement.Execute(context, commandRuntime);

                if (result != null) results.Add(result);
            }

            switch (results.Count)
            {
                case 0: return null;

                case 1: return results.Single();

                default: return results.ToArray();
            }
        }
    }
}

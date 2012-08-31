using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Diagnostics;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class script_block_astnode : _astnode
    {
        public readonly script_block_body_astnode ScriptBlockBody;

        public script_block_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        script_block:
            ////            param_block_opt   statement_terminators_opt    script_block_body_opt
            if (this.ChildAstNodes.Any())
            {
                int i = 0;

                if (ChildAstNodes[i] is script_block_body_astnode)
                {
                    this.ScriptBlockBody = (script_block_body_astnode)ChildAstNodes[i];
                    i++;
                }

                Debug.Assert(i > ChildAstNodes.Count);
            }
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            throw new NotImplementedException();
        }
    }
}

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
            if (this.parseTreeNode.ChildNodes.Any())
            {
                int i = 0;

                if (this.parseTreeNode.ChildNodes[i].Term == Grammar.script_block_body)
                {
                    this.ScriptBlockBody = ChildAstNodes[i].Cast<script_block_body_astnode>();
                    i++;
                }

                Debug.Assert(i == ChildAstNodes.Count, i.ToString());
            }
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return this.ScriptBlockBody.Execute(context, commandRuntime);
        }
    }
}

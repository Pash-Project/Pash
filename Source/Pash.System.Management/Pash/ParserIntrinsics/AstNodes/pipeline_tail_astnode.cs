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
    public class pipeline_tail_astnode : _astnode
    {
        public readonly command_astnode Command;
        public readonly pipeline_tail_astnode Pipeline;

        public pipeline_tail_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        pipeline_tail:
            ////            |   new_lines_opt   command
            ////            |   new_lines_opt   command   pipeline_tail

            this.Command = this.ChildAstNodes[0].Cast<command_astnode>();

            if (this.ChildAstNodes.Count > 1)
            {
                this.Pipeline = this.ChildAstNodes[2].Cast<pipeline_tail_astnode>();
            }

            if (this.ChildAstNodes.Count > 2) throw new InvalidOperationException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            throw new NotImplementedException();
        }
    }
}

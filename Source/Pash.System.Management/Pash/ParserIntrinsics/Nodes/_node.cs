using System;
using System.Linq;

using System.Management.Automation;
using Pash.Implementation;
using Irony.Ast;
using Irony.Parsing;

namespace Pash.ParserIntrinsics.Nodes
{
    // TODO: make it an interface, or add a field and remove this comment.
    public abstract class _node
    {
        public readonly AstContext AstContext;
        public readonly ParseTreeNode ParseTreeNode;

        public _node(AstContext astContext, ParseTreeNode parseTreeNode)
        {
            this.AstContext = astContext;
            this.ParseTreeNode = parseTreeNode;
        }

        internal abstract void Execute(ExecutionContext context, ICommandRuntime commandRuntime);

        internal abstract object GetValue(ExecutionContext context);
    }
}


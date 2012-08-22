using System;
using System.Linq;
using StringExtensions;
using System.Management.Automation;
using Pash.Implementation;
using Irony.Ast;
using Irony.Parsing;
using System.Diagnostics;

namespace Pash.ParserIntrinsics.Nodes
{
    // TODO: make it an interface, or add a field and remove this comment.
    public abstract class _node
    {
        protected readonly AstContext astContext;
        protected readonly ParseTreeNode parseTreeNode;

        public _node(AstContext astContext, ParseTreeNode parseTreeNode)
        {
            this.astContext = astContext;
            this.parseTreeNode = parseTreeNode;
        }

        // by default, forward to the child node, if there is exactly 1. This makes sense
        // for nodes that don't have a semantic impact. Somewhere, though, you have to override
        // and implement! 
        //
        // Rules with more than one child must override.
        [DebuggerStepThrough]
        internal virtual void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.parseTreeNode.ChildNodes.Count == 1)
            {
                ((_node)this.parseTreeNode.ChildNodes.Single().AstNode).Execute(context, commandRuntime);
            }
            else throw new NotImplementedException();
        }

        [DebuggerStepThrough]
        internal virtual object GetValue(ExecutionContext context)
        {
            if (this.parseTreeNode.ChildNodes.Count == 1)
            {
                var childNode = this.parseTreeNode.ChildNodes.Single();
                if (childNode.AstNode == null)
                {
                    throw new NotImplementedException("AST not implemented for '{0}'. Parent node '{1}' should implement `GetValue()`".FormatString(childNode, this));
                }
                return ((_node)childNode.AstNode).GetValue(context);
            }
            else throw new NotImplementedException();
        }
    }
}

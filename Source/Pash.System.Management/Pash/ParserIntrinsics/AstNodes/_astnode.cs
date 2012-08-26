using System;
using System.Linq;
using Extensions.String;
using System.Management.Automation;
using Pash.Implementation;
using Irony.Ast;
using Irony.Parsing;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Pash.ParserIntrinsics.AstNodes
{
    // TODO: make it an interface, or add a field and remove this comment.
    public class _astnode
    {
        protected readonly AstContext astContext;
        protected readonly ParseTreeNode parseTreeNode;

        public _astnode(AstContext astContext, ParseTreeNode parseTreeNode)
        {
            this.astContext = astContext;
            this.parseTreeNode = parseTreeNode;
        }

        public string Text { get { return parseTreeNode.Token.Text; } }

        // by default, forward to the child node, if there is exactly 1. This makes sense
        // for nodes that don't have a semantic impact. Somewhere, though, you have to override
        // and implement! 
        //
        // Rules with more than one child must override.
        [DebuggerStepThrough]
        internal virtual object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.parseTreeNode.ChildNodes.Count == 1)
            {
                var childNode = this.parseTreeNode.ChildNodes.Single();

                if (childNode.AstNode == null)
                {
                    throw new NotImplementedException("AST not implemented for '{0}'. Parent node '{1}' should implement `Execute()`".FormatString(childNode, this));
                }

                return ((_astnode)this.parseTreeNode.ChildNodes.Single().AstNode).Execute(context, commandRuntime);
            }
            else throw new NotImplementedException("AST not implemented for '{0}', because it has {1} children.. Implement `Execute()`".FormatString(this.parseTreeNode, this.parseTreeNode.ChildNodes.Count));
        }
    }
}

using System;
using GoldParser;
using Pash.Implementation;
using System.Management.Automation;

namespace Pash.ParserIntrinsics.Nodes
{
    public abstract partial class ASTNode
    {
        public bool IsTerminal
        {
            get
            {
                if (this is TerminalNode) return true;
                if (this is NonTerminalNode) return false;
                throw new Exception("Don't derive directly from ASTNode");
            }
        }

        protected object Token(Parser theParser, int index)
        {
            return theParser.GetReductionSyntaxNode(index);
        }

        protected ASTNode Node(Parser theParser, int index)
        {
            return (ASTNode)theParser.GetReductionSyntaxNode(index);
        }

        internal abstract void Execute(ExecutionContext context, ICommandRuntime commandRuntime);

        protected object _value = null;
        internal virtual object GetValue(ExecutionContext context)
        {
            return _value;
        }
    }
}


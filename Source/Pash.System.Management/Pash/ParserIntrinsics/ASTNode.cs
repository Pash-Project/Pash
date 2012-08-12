using System;
using GoldParser;
using Pash.Implementation;
using System.Management.Automation;

namespace Pash.ParserIntrinsics
{
    public abstract partial class ASTNode
    {
        public abstract bool IsTerminal
        {
            get;
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


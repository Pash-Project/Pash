using System;
using System.Linq;
using System.Linq;

using System.Management.Automation;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.Nodes
{
    // TODO: make it an interface, or add a field and remove this comment.
    public abstract class _node
    {
        internal abstract void Execute(ExecutionContext context, ICommandRuntime commandRuntime);

        internal abstract object GetValue(ExecutionContext context);
    }
}


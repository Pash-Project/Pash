using System;
using System.Management.Automation;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    public class RangeNode : NonTerminalNode
    {
        private ASTNode lValue;
        private ASTNode rValue;

        public RangeNode(Parser theParser)
            : base(theParser)
        {
            lValue = Node(theParser, 0);
            rValue = Node(theParser, 2);
        }

        public override string ToString()
        {
            return lValue.ToString() + " = " + rValue.ToString();
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if ((lValue is NumberNode) && (rValue is NumberNode))
            {
                int lNum = (int)((NumberNode) lValue).GetValue(context);
                int rNum = (int)((NumberNode) rValue).GetValue(context);

                if (lNum <= rNum)
                {
                    for(; lNum <= rNum; lNum++)
                    {
                        commandRuntime.WriteObject(lNum);
                    }
                }
                else
                {
                    for(;lNum >= rNum; lNum--)
                    {
                        commandRuntime.WriteObject(lNum);
                    }
                }

                return;
            }

            throw new InvalidOperationException("Can't execute 'range operator' for non-number values");
        }
    }
}
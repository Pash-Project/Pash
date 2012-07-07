using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    public class ArrayNode : NonTerminalNode
    {
        private ASTNode lValue;
        private ASTNode rValue;
        private List<PSObject> _array;

        public ArrayNode(Parser theParser)
            : base(theParser)
        {
            _array = new List<PSObject>();

            lValue = Node(theParser, 0);
            rValue = Node(theParser, 2);
        }

        private void AddItem(object value)
        {
            if (value is IEnumerable)
            {
                foreach (object obj in (IEnumerable)value)
                {
                    _array.Add(PSObject.AsPSObject(obj));
                }
            }
            else
            {
                _array.Add(PSObject.AsPSObject(value));
            }
        }

        public override string ToString()
        {
            return lValue.ToString() + ", " + rValue.ToString();
        }

        internal override object GetValue(ExecutionContext context)
        {
            AddItem(lValue.GetValue(context));
            AddItem(rValue.GetValue(context));

            return _array;
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            commandRuntime.WriteObject(GetValue(context), true);
        }
    }
}
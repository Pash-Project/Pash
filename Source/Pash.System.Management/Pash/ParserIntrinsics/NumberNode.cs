using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using GoldParser;
using Pash.Implementation;
using System.Globalization;

namespace Pash.ParserIntrinsics
{
    internal class NumberNode : TerminalNode
    {
        public NumberNode(Parser theParser)
            : base(theParser)
        {
        }

        internal override object GetValue(ExecutionContext context)
        {
            if (_value == null)
            {
                // TODO: check the format of the number with RegEx. Possible: int, hex, real, etc...

                // TODO: deal with Octals?

                if (Text.StartsWith("0x"))
                    _value = Int32.Parse(Text.Substring(2), NumberStyles.HexNumber);
                else 
                    _value = Int32.Parse(Text, NumberStyles.AllowHexSpecifier);
            }

            return _value;
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue((context)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    internal class StringNode : TerminalNode
    {
        public StringNode(Parser theParser)
            : base(theParser)
        {
            // strip the single quotes from the text

            if (string.IsNullOrEmpty(Text))
                return;

            if (Text[0] == '\'')
                Text = Text.Substring(1);

            if (Text[Text.Length - 1] == '\'')
                Text = Text.Substring(0, Text.Length - 1);
        }

        internal override object GetValue(ExecutionContext context)
        {
            return Text;
        }

        public override string ToString()
        {
            return "'" + Text + "'";
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(GetValue(context));
        }
    }
}

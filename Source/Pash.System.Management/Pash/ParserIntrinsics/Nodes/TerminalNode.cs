using System;
using GoldParser;
using Pash.Implementation;
using System.Management.Automation;

namespace Pash.ParserIntrinsics.Nodes
{
    /// <summary>
    /// Derive this class for Terminal AST Nodes
    /// </summary>
    public partial class TerminalNode : ASTNode
    {
        public Symbol Symbol { get; private set; }
        public string Text { get; protected set; }
        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }

        public TerminalNode(Parser theParser)
        {
            Symbol = theParser.TokenSymbol;
            //Text = theParser.TokenSymbol.ToString();
            Text = (string)Token(theParser, 0);
            LineNumber = theParser.LineNumber;
            LinePosition = theParser.LinePosition;
        }

        public override string ToString()
        {
            return Text;
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            context.outputStreamWriter.Write(Text);
        }
    }
}


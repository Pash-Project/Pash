using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;
using Pash.ParserIntrinsics;

// use aliases to not confuse ironys parser class with this one
using IronyParser = Irony.Parsing.Parser;
using ParseTree = Irony.Parsing.ParseTree;

namespace Pash.Implementation
{
    public class Parser
    {
        // This is a little weird. Hopefully someone will have a bright idea
        // of how to refactor this to separate the grammar from the parser. Or
        // merge them. Or whatever.
        public static readonly PowerShellGrammar Grammar;
        public static IronyParser IronyParser;

        static Parser()
        {
            Grammar = new PowerShellGrammar();
            IronyParser = new IronyParser(Grammar);
        }

        public static ScriptBlockAst ParseInput(string input)
        {
            var parseTree = Parse(input);
            return new AstBuilder(Grammar).BuildScriptBlockAst(parseTree.Root);
        }

        private static ParseTree Parse(string input)
        {
            ParseTree parseTree = null;
            try
            {
                parseTree = IronyParser.Parse(input);
            }
            catch (Exception)
            {
                var msg = "The parser internally crashed and gets reinitialized." + Environment.NewLine +
                    "Although this shouldn't happen, it's likely that it happened because of invalid syntax.";
                IronyParser = new IronyParser(Grammar);
                throw new InvalidOperationException(msg);
            }
            if (parseTree.HasErrors())
            {
                var logMessage = parseTree.ParserMessages.First();
                throw new ParseException(logMessage.Message, logMessage.Location.Line, logMessage.Location.Column);
            }
            return parseTree;
        }
    }
}


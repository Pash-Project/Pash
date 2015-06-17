using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;
using Pash.ParserIntrinsics;

// use aliases to not confuse ironys parser class with this one
using IronyParser = Irony.Parsing.Parser;
using ParseTree = Irony.Parsing.ParseTree;
using ParseTreeStatus = Irony.Parsing.ParseTreeStatus;
using ParseMode = Irony.Parsing.ParseMode;
using Token = Irony.Parsing.Token;

namespace Pash.Implementation
{
    public static class Parser
    {
        public static readonly string[] ControlStatementKeywords = new [] {
            "do", "while", "for", "foreach", "if", "else", "elseif", "switch", "try", "catch", "trap",
            "function", "filter", "begin", "process", "end"
        };
        public static readonly PowerShellGrammar Grammar;
        public static IronyParser IronyParser;

        static Parser()
        {
            Grammar = new PowerShellGrammar();
            IronyParser = new IronyParser(Grammar);
        }

        public static ScriptBlockAst ParseInput(string input)
        {
            var parseTree = Parse(input, false);
            return new AstBuilder(Grammar, parseTree).BuildScriptBlockAst(parseTree.Root);
        }

        public static bool TryParsePartialInput(string input, out ScriptBlockAst scriptBlock)
        {
            string tmp;
            return TryParsePartialInput(input, out scriptBlock, out tmp);
        }

        public static bool TryParsePartialInput(string input, out ScriptBlockAst scriptBlock,
            out string lastControlStmt)
        {
            scriptBlock = null;
            lastControlStmt = "";
            var parseTree = Parse(input, true);
            if (parseTree.Status.Equals(ParseTreeStatus.Parsed))
            {
                scriptBlock = new AstBuilder(Grammar, parseTree).BuildScriptBlockAst(parseTree.Root);
                return true;
            }
            // if we're here we only partially parsed, because Parse would have thrown on error
            var reversedTokens = parseTree.Tokens.Reverse<Token>();
            foreach (var token in reversedTokens)
            {
                if (token.KeyTerm != null &&
                    ControlStatementKeywords.Contains(token.KeyTerm.Text, StringComparer.InvariantCultureIgnoreCase))
                {
                    lastControlStmt = token.KeyTerm.Text;
                    break;
                }
            }

            return false;
        }

        private static ParseTree Parse(string input, bool allowPartial)
        {
            ParseTree parseTree = null;
            IronyParser.Context.Mode = allowPartial ? ParseMode.CommandLine : ParseMode.File;
            try
            {
                parseTree = IronyParser.Parse(input);
            }
            catch (Exception)
            {
                var msg = "The parser internally crashed and gets reinitialized." + Environment.NewLine +
                    "Although this shouldn't happen, it's likely that it happened because of invalid syntax.";
                IronyParser = new IronyParser(Grammar);
                throw new ParseException(msg);
            }

            if (parseTree.HasErrors()) // ParseTreeStatus is Error
            {
                var logMessage = parseTree.ParserMessages.First();
                throw new ParseException(logMessage.Message, logMessage.Location.Line, logMessage.Location.Column,
                                         parseTree.SourceText);
            }
            return parseTree;
        }
    }
}

